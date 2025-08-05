using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace YiSha.Util
{
    public class CaptchaHelper
    {
        #region 得到驗證碼
        /// <summary>
        /// Tuple第一個值是表達式，第二個值是表達式結果
        /// </summary>
        /// <returns></returns>
        public static Tuple<string, int> GetCaptchaCode()
        {
            int value = 0;
            char[] operators = { '+', '-', '*' };
            string randomCode = string.Empty;
            Random random = new Random();

            int first = random.Next() % 10;
            int second = random.Next() % 10;
            char operatorChar = operators[random.Next(0, operators.Length)];
            switch (operatorChar)
            {
                case '+': value = first + second; break;
                case '-':
                    // 第1個數要大於第二個數
                    if (first < second)
                    {
                        int temp = first;
                        first = second;
                        second = temp;
                    }
                    value = first - second;
                    break;
                case '*': value = first * second; break;
            }

            char code = (char)('0' + (char)first);
            randomCode += code;
            randomCode += operatorChar;
            code = (char)('0' + (char)second);
            randomCode += code;
            randomCode += "=?";
            return new Tuple<string, int>(randomCode, value);
        }
        #endregion

        #region 生成驗證碼圖片
        /// <summary>
        /// 生成驗證碼圖片
        /// </summary>
        /// <param name="randomCode"></param>
        /// <returns></returns>
        public static byte[] CreateCaptchaImage(string randomCode)
        {
            const int randAngle = 45; //随機转動角度
            int mapwidth = (int)(randomCode.Length * 16);
            Bitmap map = new Bitmap(mapwidth, 28);//創建圖片背景
            Graphics graph = Graphics.FromImage(map);
            graph.Clear(Color.AliceBlue);//清除画面，填充背景

            Random random = new Random();
            //背景噪點生成，為了在白色背景上顯示，尽量生成深色
            //int intRed = random.Next(256);
            //int intGreen = random.Next(256);
            //int intBlue = (intRed + intGreen > 400) ? 0 : 400 - intRed - intGreen;
            //intBlue = (intBlue > 255) ? 255 : intBlue;

            //Pen blackPen = new Pen(Color.FromArgb(intRed, intGreen, intBlue), 0);
            //for (int i = 0; i < 50; i++)
            //{
            //    int x = random.Next(0, map.Width);
            //    int y = random.Next(0, map.Height);
            //    graph.DrawRectangle(blackPen, x, y, 1, 1);
            //}
            //绘制干扰曲線
            for (int i = 0; i < 2; i++)
            {
                Point p1 = new Point(0, random.Next(map.Height));
                Point p2 = new Point(random.Next(map.Width), random.Next(map.Height));
                Point p3 = new Point(random.Next(map.Width), random.Next(map.Height));
                Point p4 = new Point(map.Width, random.Next(map.Height));
                Point[] p = { p1, p2, p3, p4 };
                using (Pen pen = new Pen(Color.Gray, 1))
                {
                    graph.DrawBeziers(pen, p);
                }
            }

            //文字距中
            using (StringFormat format = new StringFormat(StringFormatFlags.NoClip))
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                //定義顏色
                Color[] c = { Color.Black, Color.Red, Color.DarkBlue, Color.Green, Color.Orange, Color.Brown, Color.DarkCyan, Color.Purple };
                //定義字體
                string[] fonts = { "Verdana", "Microsoft Sans Serif", "Comic Sans MS", "Arial", "宋體" };
                int cindex = random.Next(7);

                //驗證碼旋转，防止機器識別
                char[] chars = randomCode.ToCharArray();//拆散字符串成單字符數組
                foreach (char t in chars)
                {
                    int findex = random.Next(5);
                    using (Font font = new Font(fonts[findex], 14, FontStyle.Bold))//字體樣式(參數2為字體大小)
                    {
                        using (Brush brush = new SolidBrush(c[cindex]))
                        {
                            Point dot = new Point(14, 14);
                            float angle = random.Next(-randAngle, randAngle);//转動的度數
                            if (t == '+' || t == '-' || t == '*')
                            {
                                //加减乘運算符不進行旋转
                                graph.TranslateTransform(dot.X, dot.Y);//移動光標到指定位置
                                graph.DrawString(t.ToString(), font, brush, 1, 1, format);
                                graph.TranslateTransform(-2, -dot.Y);//移動光標到指定位置，每個字符紧凑顯示，避免被軟體識別
                            }
                            else
                            {
                                graph.TranslateTransform(dot.X, dot.Y);//移動光標到指定位置
                                graph.RotateTransform(angle);
                                graph.DrawString(t.ToString(), font, brush, 1, 1, format);
                                graph.RotateTransform(-angle);//转回去
                                graph.TranslateTransform(-2, -dot.Y);//移動光標到指定位置，每個字符紧凑顯示，避免被軟體識別
                            }
                        }
                    }
                }
            }
            //生成圖片
            using (MemoryStream ms = new MemoryStream())
            {
                map.Save(ms, ImageFormat.Gif);

                graph.Dispose();
                map.Dispose();
                return ms.GetBuffer();
            }
        }
        #endregion
    }
}
