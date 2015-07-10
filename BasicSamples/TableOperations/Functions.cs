using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;

namespace TableOperations
{
    public class WordCount : TableEntity
    {
        public string Word { get; set; }

        public int Count { get; set; }
    }

    public class LogEntry : TableEntity
    {
        public DateTime Date { get; set; }
    }

    public class Functions
    {
        /// <summary>
        /// Creates the frequency table for the words in the input string and then splits the phrase in words
        /// </summary>
        public static void CountAndSplitInWords(
            [QueueTrigger("textInput")] string textInput, 
            [Table("words")] CloudTable wordsTable, 
            [Queue("words")] ICollector<string> wordsQueue)
        {
            // Normalize the capitalization
            textInput = textInput.ToLower();

            // Split in words (assume words are only delimited by space)
            string[] wordsCollection = textInput.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Create word groups (one group/word)
            var wordCount = wordsCollection.GroupBy(w => w);

            foreach (var group in wordCount)
            {
                // The data in the storage table has 
                //      PartitionKey = the first letter of the word
                //      RowKey = the full word
                string partitonKey = group.Key[0].ToString();
                string rowKey = group.Key;

                WordCount count = new WordCount()
                {
                    PartitionKey = partitonKey,
                    RowKey = rowKey,

                    Word = group.Key,
                    Count = group.Count()
                };

                TableOperation operation = TableOperation.InsertOrReplace(count);
                wordsTable.Execute(operation);
            }

            // Enqueue distinct words (no duplicates)
            foreach (string word in wordCount.Select(g => g.Key))
            {
                wordsQueue.Add(word);
            }
        }

        /// <summary>
        /// Counts the frequency of characters in a word (triggered by messages created by "CountAndSplitInWords")
        /// </summary>
        public static void CharFrequency(
            [QueueTrigger("words")] string word, 
            TextWriter log)
        {
            // Create a dictionary of character frequencies
            //      Key = the character
            //      Value = number of times that character appears in a word
            IDictionary<char, int> frequency = word
                .GroupBy(c => c)
                .ToDictionary(group => group.Key, group => group.Count());

            log.WriteLine("The frequency of letters in the word \"{0}\" is: ", word);
            foreach (var character in frequency)
            {
                log.WriteLine("{0}: {1}", character.Key, character.Value);
            }
        }

        [NoAutomaticTrigger]
        public static void ManualTrigger([Table("log")] CloudTable logTable)
        {
            DateTime dt = DateTime.Now;

            LogEntry log = new LogEntry()
            {
                PartitionKey = dt.Year.ToString(),
                RowKey = dt.Month.ToString(),
                Date = dt
            };

            TableOperation operation = TableOperation.InsertOrReplace(log);

            logTable.Execute(operation);
        }
    }
}
