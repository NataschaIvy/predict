using System.IO;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using camapi.core.Models.ML;
using camapi.core.Models.Uploads;
using camapi.core.Extensions.ImageHelpers;

namespace camapi.predict.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PredictController : ControllerBase
    {

        #region Private Members
        
        /// <summary>
        /// /// logger
        /// </summary>
        private readonly ILogger<PredictController> _logger;

        /// <summary>
        /// the predection engine pool
        /// </summary>
        private readonly PredictionEnginePool<InMemoryImageData, ImagePrediction> _engine;

        #endregion
        
        /// <summary>
        /// controller that accepts a post with any number of image files
        /// labels them using machine learning classifiers
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="enginePool"></param>
        public PredictController(
            ILogger<PredictController> logger, 
            PredictionEnginePool<InMemoryImageData, ImagePrediction> enginePool)
        {
            _logger     = logger;            
            _engine     = enginePool;
        }

        /// <summary>
        /// predict labels for a list of image files
        /// </summary>
        /// <param name="files"></param>
        /// <returns>a list of predicted labels</returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            //make sure that we have form files
            if (Request == null || Request.Form == null || Request.Form.Files == null)
            {
                return BadRequest("Missing form files");
            }

            //prepare the result
            var result = new List<PredictedResult>();

            foreach(var file in Request.Form.Files)
            {
                using(var fileStream = new MemoryStream())
                {
                    await file.CopyToAsync(fileStream);                    
                    
                    //prep the file for prediction
                    byte[] fileData = fileStream.ToArray();

                    if(!fileData.IsValidImage())
                    {
                        return StatusCode((int)HttpStatusCode.UnsupportedMediaType, 
                            new { error = "Unsupported media format.  Files must be of type .jpeg or .png", filename = file.FileName });
                    }

                    var imageInputData = new InMemoryImageData(image:fileData, label: null, imageFileName: null);

                    //time and run the prediction
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    var predection = _engine.Predict(imageInputData);
                    watch.Stop();
                    var ellapsedTime = watch.ElapsedMilliseconds;

                    //create the result
                    var predictionResult = new PredictedResult                    
                    {
                        PredictedLabel      = predection.PredictedLabel,
                        ConfidenceScore     = predection.Score.Max(),
                        ElapsedMilliseconds = ellapsedTime
                    };

                    result.Add(predictionResult);       
                }
            }

            return Ok(result);
            
        }
    }
}
