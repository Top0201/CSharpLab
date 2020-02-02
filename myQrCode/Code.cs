using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using System;
using System.Drawing;
using System.Text;
using System.IO;
using System.Drawing.Imaging;

namespace myQrCode
{
    class Code
    {
        protected void CreateQrCode(String text)
        {//用于生成二维码的函数

            QrEncoder qrEncoder = new QrEncoder(ErrorCorrectionLevel.M);
            //二维码的容错率设置
            QrCode qrCode = qrEncoder.Encode(text);
            //用命令行字符串编码
            for (int i = 0; i < qrCode.Matrix.Height; i++)
            {
                for (int j = 0; j < qrCode.Matrix.Width; j++)
                {
                    char charToPrint = qrCode.Matrix[i, j] ? '□' : '■'
;
                    Console.Write(charToPrint);
                    //将二维码写入到控制台

                }
                Console.WriteLine();

            }
        }

        protected void SaveQrCode(string text,string filename)
        {//用于保存二维码图像的函数

            QrEncoder qrEncoder = new QrEncoder(ErrorCorrectionLevel.M);
            //设置二维码错误修正率
            QrCode qrCode = qrEncoder.Encode(text);
            //使用text参数进行编码
            GraphicsRenderer renderer = new GraphicsRenderer(new FixedModuleSize(5, QuietZoneModules.Two),
                Brushes.Black, Brushes.White);
            //实例化Renderer对象，
            string fileurl = "C://Users//Top//Pictures//Saved Pictures//" + filename+".png";
            using (FileStream stream = new FileStream(fileurl, FileMode.Create))
            {
                renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, stream);
                //将二维码矩阵写入文件流，设置图片格式为png
            }
            Console.WriteLine("Your QrCode pictures are stored in " + fileurl);
        }


        static void Main(string []args)
        {
            Code code = new Code();
            if (args.Length > 0 && args.Length < 100)
            {

                string sampleText = null;
                for (int i = 0; i < args.Length; i++)//得到命令行字符串
                {
                    sampleText += args[i];
                }
                int loc = sampleText.IndexOf("-f");//寻找文件名的开始标识
                if (loc != -1)//命令行存在文件名
                {
                    string filename = sampleText.Substring(loc + 2, sampleText.Length-loc-2);//截取文件名
                    int line = 0;
                    //要先判断文件是否存在，再打开
                    try
                    {
                        FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read);
                        //文件状态为打开且读入
                        StreamReader read = new StreamReader(file, Encoding.Default);
                        //创建读入流，设置读取文件编码为默认值
                        string command;

                        while ((command = read.ReadLine()) != null)
                        {
                            line++;
                            string temp = command.Substring(0, 4);
                            code.SaveQrCode(command,string.Format("{0:000}",line)+temp);
                        }

                        file.Close();

                    }
                    catch (IOException e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    
                }
                else 
                {
                    code.CreateQrCode(sampleText);
                    Console.WriteLine("Please press and key to quit.");
                }
            }
            else
            {
                Console.WriteLine("Please input your command lines to create your QrCode.");

            }
            Console.WriteLine("Your QrCodes are created.");
            Console.Read();
        }
    }
}

    
