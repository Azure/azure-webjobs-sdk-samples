using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhluffyShuffyWebData
{
    public interface IImageStorage
    {
        IEnumerable<string> GetShufflesOlderThan(DateTime date);

        IEnumerable<Uri> GetAllShuffleParts(string shuffleId);

        Uri GetImageLink(string shuffleId);

        bool IsReadonly(string shuffleId);

        void AddNewPart(string shuffleId, string fileName, Stream fileStream);

        void RequestShuffle(string shuffleId);

        void Delete(string shuffleId);
    }
}
