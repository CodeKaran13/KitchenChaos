using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TensorFlowLite;
using UnityEngine;
using UnityEngine.UI;

namespace TensorFlowLite
{
    internal class AgeEstimations : BaseImagePredictor<float>
    {
        private int[][] classMapping = new int[][] {
            new int[]{0, 2},
            new int[]{2, 4},
            new int[]{4, 8},
            new int[]{8, 13},
            new int[]{13, 18},
            new int[]{18, 21},
            new int[]{21, 25},
            new int[]{25, 28},
            new int[]{28, 32},
            new int[]{32, 36},
            new int[]{36, 40},
            new int[]{40, 45},
            new int[]{45, 50},
            new int[]{50, 60},
            new int[]{60, 70},
            new int[]{70, 120}
        };

        internal class Result
        {
            public float age;
        }

        private float[,] output = new float[1, 1];

        private static string modelPath = "model_reg_mug.tflite.enc";

        internal AgeEstimations() : this(modelPath)
        {
        }

        public AgeEstimations(string modelPath) : base(modelPath, Accelerator.NONE)
        {
            Debug.Log("Model created from decrypted data");
        }

        public AgeEstimations(byte[] modelData) : base(modelData, BaseImagePredictor<float>.CreateOptions(Accelerator.NONE))
        {

        }

        public void estimateAge(Texture texture)
        {
            Debug.Log("Estimating");
            var croppingRect = new Rect(120, 120, 400, 400);

            resizeOptions = new TextureResizer.ResizeOptions()
            {
                aspectMode = AspectMode.Fill,
                rotationDegree = 0,
                mirrorHorizontal = false,
                mirrorVertical = false,
                width = (int)croppingRect.width,
                height = (int)croppingRect.height,
            };

            var texture2d = getTexture2d(texture);
            var pixels = texture2d.GetPixels((int)croppingRect.x, (int)croppingRect.y, (int)croppingRect.width, (int)croppingRect.height, 0);

            //CROP IMAGE TO FIT SQUARE RATIO (cropSize)
            var img = new Texture2D((int)croppingRect.width, (int)croppingRect.height, TextureFormat.RGBA32, false);

            img.SetPixels(0, 0, (int)croppingRect.width, (int)croppingRect.height, pixels, 0);
            img.Apply();

            ToTensor(img, inputTensor);

            interpreter.SetInputTensorData(0, inputTensor);
            interpreter.Invoke();

            interpreter.GetOutputTensorData(0, output);
        }

        public void estimateAgeFromCroppedImage(Texture texture)
        {
            ToTensor(texture, inputTensor);

            interpreter.SetInputTensorData(0, inputTensor);
            interpreter.Invoke();

            interpreter.GetOutputTensorData(0, output);
        }

        public Texture2D getTexture2d(Texture texture)
        {
            var oldRen = RenderTexture.active;
            RenderTexture tex = resizer.Resize(texture, resizeOptions);
            RenderTexture.active = tex;
            var texture2D = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false, false);
            texture2D.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = oldRen;
            return texture2D;
        }

        public override void Invoke(Texture inputTex)
        {
            estimateAge(texture: inputTex);
        }

        public Result GetResult()
        {
            float predictedAge = 200;
            float regressionOutput = output[0, 0];
            if (regressionOutput < 0.0f)
            {
                predictedAge = 0.0f;
            }
            else
            {
                for (int i = 0; i < classMapping.Length; i++)
                {
                    if (regressionOutput >= i && regressionOutput < i + 1)
                    {
                        float ageRange = classMapping[i][1] - classMapping[i][0];
                        predictedAge = classMapping[i][0] + (regressionOutput - i) * ageRange;
                    }
                }
            }

            return new Result()
            {
                age = predictedAge
            };
        }
    }
}