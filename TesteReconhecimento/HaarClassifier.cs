using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using Emgu.CV.CvEnum;
using System.IO;
using Emgu.CV.Util;

namespace TesteReconhecimento
{
    class HaarClassifier
    {
        private CascadeClassifier _Classifier;
        private string _HaarFile;

        public string HaarFile
        {
            get { return _HaarFile; }
            set
            {
                _HaarFile = value;
                _Classifier = new CascadeClassifier(value);
            }
        }
        public double Scale { get; set; }
        public int Neighboors { get; set; }
        public Size TrainedSize { get; set; }

        public HaarClassifier() {
            Scale = 1.1;
            Neighboors = 3;
            TrainedSize = Size.Empty; }

        private MemoryStream DrawMatches(Mat mat)
        {

            Rectangle[] matches;
            using (UMat img = mat.GetUMat(AccessType.ReadWrite))
                matches = GetMatches(img);

            foreach (Rectangle rect in matches)
                CvInvoke.Rectangle(mat, rect, new MCvScalar(0, 0, 255), 2);

            MemoryStream result = null;
            using (VectorOfByte vb = new VectorOfByte())
            {
                CvInvoke.Imencode(".jpg", mat, vb);
                byte[] rawData = vb.ToArray();
                result = new MemoryStream(rawData);
            }

            return result;
        }

        public async Task<MemoryStream> GetImageWithMatches(string file)
        {
            using (Mat mat = CvInvoke.Imread(file, ImreadModes.Color))
            {
                Task<MemoryStream> t = new Task<MemoryStream>(() => DrawMatches(mat));
                t.Start();
                var taskResult = await t;
                return taskResult;
            }
        }

        public async Task<MemoryStream> GetImageWithMatches(MemoryStream image)
        {
            byte[] data = image.ToArray();
            using (Mat mat = new Mat())
            {
                CvInvoke.Imdecode(data, ImreadModes.Color, mat);
                Task<MemoryStream> t = new Task<MemoryStream>(() => DrawMatches(mat));
                t.Start();
                var taskResult = await t;
                return taskResult;
            }
        }

        public Rectangle[] GetMatches(string file)
        {
            Mat mat = CvInvoke.Imread(file, ImreadModes.Color);
            Rectangle[] matches;
            using (UMat img = mat.GetUMat(AccessType.ReadWrite))
                matches = GetMatches(img);

            return matches;
        }
        public Rectangle[] GetMatches(IInputArray image)
        {
            using (UMat ugray = new UMat())
            {
                //grayscale
                CvInvoke.CvtColor(image, ugray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                //normalizes brightness and increases contrast of the image
                CvInvoke.EqualizeHist(ugray, ugray);

                return _Classifier.DetectMultiScale(ugray, Scale, Neighboors, TrainedSize);
            }
        }
    }
}
