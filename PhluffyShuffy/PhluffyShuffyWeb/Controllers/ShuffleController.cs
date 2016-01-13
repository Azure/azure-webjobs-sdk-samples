using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using PhluffyShuffyWeb.Models;
using PhluffyShuffyWebData;

namespace PhluffyShuffyWeb.Controllers
{
    public class ShuffleController : Controller
    {
        private readonly IImageStorage storage;

        public ShuffleController()
            : this(new AzureImageStorage(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString))
        {
        }

        public ShuffleController(IImageStorage imageStorage)
        {
            if (imageStorage == null)
            {
                throw new ArgumentNullException("imageStorage");
            }

            this.storage = imageStorage;
        }

        public ViewResult Index(string id = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                // Create a string that is valid for azure container naming
                id = Guid.NewGuid().ToString("N").ToLower();
            }

            ImageModel model = new ImageModel()
            {
                Id = id,
                ImageParts = this.storage.GetAllShuffleParts(id),
                IsReadonly = this.storage.IsReadonly(id)
            };

            return View(model);
        }

        [HttpGet]
        public JsonResult ShufflePath(string shuffleId)
        {
            Uri shuffleLink = this.storage.GetImageLink(shuffleId);

            JsonResult result;
            if (shuffleLink == null)
            {
                this.Response.StatusCode = 404;
                result = Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                result = Json(shuffleLink.ToString(), JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        [HttpPost]
        public ActionResult Upload(string shuffleId, IEnumerable<HttpPostedFileBase> files)
        {
            foreach(HttpPostedFileBase file in files)
            { 
                this.storage.AddNewPart(shuffleId, file.FileName, file.InputStream);
            }

            return RedirectToAction("Index", new { id = shuffleId });
        }

        [HttpPost]
        public ActionResult Create(string shuffleId)
        {
            this.storage.RequestShuffle(shuffleId);

            return RedirectToAction("Index", new { id = shuffleId });
        }
    }
}