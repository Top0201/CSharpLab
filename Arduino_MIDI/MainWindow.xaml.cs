using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Windows.Threading;
using ZedGraph;
using System.Windows.Forms;
using Binding = System.Windows.Data.Binding;
using TextBox = System.Windows.Controls.TextBox;
using ComboBox = System.Windows.Controls.ComboBox;
using MessageBox = System.Windows.MessageBox;


namespace Arduino_MIDI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort port = null;//serialport对象
        midi Receive = null;//pc接收arduino对象
        midi Send = null;//pc发送命令对象
        int tickStart = 0;//计时器
        log _log = null;//日志字符串类
        
        public MainWindow()
        {
            InitializeComponent();
            Receive = new midi();
            Send = new midi();

            //将arduino发送给pc的数据绑定成绑定源
            Binding binding = new Binding("ReceiveMessage");
            binding.Source = Receive;
            //设置绑定数据修改模式
            binding.Mode = BindingMode.OneWay;
            //将绑定目标设为显示arduino返回数据的textbox
            returnData.SetBinding(TextBox.TextProperty, binding);


            //将pc发送给arduino的数据设为绑定源
            binding = new Binding("SendMessage");
            binding.Source = Send;
            binding.Mode = BindingMode.OneWayToSource;
            //将绑定目标设为可写入pc发送命令的textbox
            sendData.SetBinding(TextBox.TextProperty, binding);

            //绑定温度至温度显示textblock中
            binding = new Binding("getDegree");
            binding.Source = Receive;
            binding.Mode = BindingMode.OneWay;
            tempre.SetBinding(TextBlock.TextProperty, binding);

            //绑定光强至光强显示textblock中
            binding = new Binding("getLight");
            binding.Source = Receive;
            binding.Mode = BindingMode.OneWay;
            light.SetBinding(TextBlock.TextProperty, binding);

            ////绑定led的滑动条至发送数据中
            //setBind(red);
            //setBind(green);
            //setBind(white);
            //setBind(blue);
            //setBind(yellow);

            //设定串口bps
            bps.Items.Clear();
            bps.Items.Add("9600");
            bps.Items.Add("19200");
            bps.Items.Add("38400");
            bps.Items.Add("57600");
            bps.Items.Add("115200");
            bps.Items.Add("921600");
            bps.SelectedItem = bps.Items.GetItemAt(2);

            //初始化zedGraph
            setGraph();

            //初始化log实例
            _log = new log();

        }

        private void setBind(Slider slider)
        {
            Binding binding = new Binding("Pin");
            binding.Source = Send;
            binding.Mode = BindingMode.OneWayToSource;
            slider.SetBinding(Slider.TagProperty, binding);

            binding = new Binding("State");
            binding.Source = Send;
            binding.Mode = BindingMode.OneWayToSource;
            slider.SetBinding(Slider.ValueProperty, binding);
        }

        //当前combox下拉选择串口时处理事件
        public void portName_DropDownOpened(object sender, EventArgs e)
        {
            //获取当前计算机的串行端口名的数组
            string[] portNames = SerialPort.GetPortNames();
            Console.WriteLine(portNames.Length);
            ComboBox combo = sender as ComboBox;//将sender转换成combo~
            combo.Items.Clear();//每次读取串口清楚之前的item值
            foreach(string name in portNames)
            {
                Console.WriteLine(name);
                combo.Items.Add(name);
            }

        }

        //选择串口bps
        private void bps_selectedChanged(object sender,SelectionChangedEventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            Console.WriteLine("Selected:" + combo.SelectedItem);

        }

        private void closePort()
        {
            if (port != null)
            {
                //事件dataReceived指示已通过serialPort对象表示的端口接受了数据

                port.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandler);
                port.Close();
                MessageBox.Show("断开成功");
                Console.WriteLine("Closed:" + port.ToString());
            }
        }
        //关闭串口按钮事件
        private void closedSerialPort(object sender,RoutedEventArgs e)
        {
            closePort();
        }

        //打开串口按钮事件
        private void openSerialPort(object sender,RoutedEventArgs e)
        {
            if(serialPort.SelectedItem!=null)
            {
                //打开串口时先判断当前串口是否不为null
                if(port!=null)
                {
                    port.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandler);
                    port.Close();
                }
                //更新串口，new新对象
                port = new SerialPort(serialPort.SelectedItem.ToString());
                //设定bps
                port.BaudRate = int.Parse(bps.SelectedItem.ToString());
                //设置串口校验位
                port.Parity = Parity.None;
                //设置每个字节的标准停止位数
                port.StopBits = StopBits.One;
                //设置每个字节标准数据位长度
                port.DataBits = 8;
                //设置串行端口数据传输的握手协议
                port.Handshake = Handshake.None;
                //在串行通信中是否启用请求发送rts信号
                port.RtsEnable = false;
                //设置事件发生前内部输入缓冲区中的字节数
                port.ReceivedBytesThreshold = 1;
                port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

                //打开串口
                try
                {
                    port.Open();
                    MessageBox.Show("连接成功");
                    Console.WriteLine("Open selected port:" + serialPort.SelectedItem);
                }
                catch(Exception e1)
                {
                    Console.WriteLine(e1.Message);
                }
                
            }
            else
            {
                Console.WriteLine("No selected serial port!");
                MessageBox.Show("Please select serial port", "Warning");
            }
        }


        //处理温度和光强显示
        private void display()
        {
            if((Receive.getReceive[0]&0xf0)==0xe0)
            {
                if((Receive.getReceive[0]&0xf)==0)
                {
                    Receive.getDegree = Receive.byte_to_int;
                    setReturnTextBlock(tempre, Receive.getDegree);

                }
                else if((Receive.getReceive[0]&0xf)==1)
                {
                    Receive.getLight = Receive.byte_to_int;
                    setReturnTextBlock(light, Receive.getLight);
                }
            }
        }

        //pc发送命令到arduino处理事件
        private void DataSend(object sender,RoutedEventArgs e)
        {
            byte[] data = Send.SendMessageToByte;

            //串口开启且当前发送字节数组存在
            if(data!=null && port!=null && port.IsOpen)
            {
                port.Write(data, 0, data.Length);
            }
        }
        
        //pc接收到arduino的数据处理事件
        private void DataReceivedHandler(object sender,SerialDataReceivedEventArgs e)
        {
            if(port==null)
            {
                return;
            }

            //获取接收器中数据的字节数，arduino上传给pc的数据
            int byte_size = port.BytesToRead;
            for(int i=0;i<byte_size;i++)
            {
                //从缓冲区中同步读取一个字节
                int indata = port.ReadByte();

                if((indata&0x80)==0x80)
                {
                    Receive.index = 0;
                    Receive.getReceive[Receive.index] = (byte)indata;
                    Receive.index++;
                }
                else if(Receive.index!=0 && Receive.index<Receive.getReceive.Length)
                {
                    Receive.getReceive[Receive.index] = (byte)indata;
                    Receive.index++;
                }
                if(Receive.index==3)
                {
                    string s = string.Format("\n ReceiveData:{0:X2}-{1:X2}-{2:X2},RealData=0x{3:X4}",
                        Receive.getReceive[0], Receive.getReceive[1], 
                        Receive.getReceive[2], Receive.byte_to_int);

                    //将此信息发送进returnData的textbox中
                    setReturnTextBox(returnData, s);

                    //检查温度和光强显示
                    display();
                    if((Receive.getReceive[0]&0xf0)==0xe0)
                    {
                        drawline(Receive.getReceive[0] & 0xf, Receive.byte_to_int);
                    }
                    

                    //记录日志
                    if (click == 1)
                    {
                        _log.AD = Receive.getReceive;
                        _log.PWM = Send.getSend;
                        _log.setString();

                    }         
                }
            }
        }

        //委托事件用于处理线程不同步问题
        private delegate void setTextBox(TextBox textBox, string s);
        
        //设置pc接收数据到textbox中
        public void setReturnTextBox(TextBox textBox,string s)
        {
            //检查当前textbox中的被调用线程是否是当前与之相关联的dispather的线程
            if(textBox.Dispatcher.CheckAccess())
            {
                //将pc接收到的数据加入到textbox中
                textBox.AppendText(s);
                textBox.ScrollToEnd();
            }
            else
            {
                //将函数委托给事件处理，使用当前dispatcher 调用invoke该委托事件
                setTextBox setText = new setTextBox(setReturnTextBox);
                Dispatcher.Invoke(setText, new object[] { textBox, s });
            }
        }

        //发送温度和光强至textblock中
        private delegate void setTextBlock(TextBlock textBlock, int num);

        public void setReturnTextBlock(TextBlock textBlock,int num)
        {
            if(textBlock.Dispatcher.CheckAccess())
            {
                textBlock.Text = num.ToString();
            }
            else
            {
                setTextBlock set = new setTextBlock(setReturnTextBlock);
                Dispatcher.Invoke(set, new object[] { textBlock, num });
            }
        }

        
        //窗体关闭时关闭串口
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closePort();
                
        }

        //处理滑动条事件
        private void ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            Slider slider = sender as Slider;

            //得到滑动条对应的通道号
            int pin = int.Parse(slider.Tag.ToString());
            //得到滑动条对应的value值
            int state = (int)slider.Value;

            //设置Send对象发送数据字节数组
            Send.getSend[0] = (byte)(0xd0 | pin);
            Send.getSend[1] = (byte)(state & 0x7f);
            Send.getSend[2] = (byte)((state >> 7) & 0x7f);
            //发送字节数组至arduino,进行串口是否打开的检查
            if(port!=null && port.IsOpen)
            {
                port.Write(Send.getSend, 0, Send.getSend.Length);
                rgb();
            }
            else
            {
                return;
            }
               
            //byte[] data = Send.getSend;
            //if(data!=null && port!=null)
            //{
            //    port.Write(data, 0, data.Length);

            //}
                   
        }


        //初始化zedgraph

        private void setGraph()
        {
            GraphPane graph = zedgraph.GraphPane;

            //设置标题
            graph.Title.Text = "Tempreture and light sensity";
            //设置x轴
            graph.XAxis.Title.Text = "Time/Seconds";
            //设置y轴文字
            graph.Title.Text = "Value";
            
            //设置1200点，假设每50毫秒更新一次，刚好检测1分钟

            //数据点集
            RollingPointPairList list1 = new RollingPointPairList(1200);
            RollingPointPairList list2 = new RollingPointPairList(1200);

            //刚开始增加的线数据点为空，list为空
            LineItem tempre = graph.AddCurve("Tempreture", list1, System.Drawing.Color.Blue, SymbolType.None);
            LineItem light = graph.AddCurve("Light sensity", list2, System.Drawing.Color.Red, SymbolType.None);

            //设置x轴
            graph.XAxis.Scale.Min = 0;
            graph.XAxis.Scale.MaxGrace = 0.01;
            graph.XAxis.Scale.MaxGrace = 0.01;
            graph.XAxis.Scale.Max = 30;
            graph.XAxis.Scale.MinorStep = 1;
            graph.XAxis.Scale.MajorStep = 5;

            //设置保存开始时间
            tickStart = Environment.TickCount;
            //改变轴的刻度
            zedgraph.AxisChange();
     
        }

        //画温度和光强曲线函数
        private void drawline(int channel,double data)
        {
            //确保curvelist不为空
            if(zedgraph.GraphPane.CurveList.Count<=0)
            {
                return;
            }

            LineItem line;
            if (channel==0)
            {
                //取graph第一个曲线，在GraphPane.CurveList集合中查找CurveItem,对应于温度曲线
                line = zedgraph.GraphPane.CurveList[0] as LineItem;
            }
            else
            {
                //得到对应于光强的曲线
                line = zedgraph.GraphPane.CurveList[1] as LineItem;
            }

            if(line==null)
            {
                return;
            }

            //在CurveItem中访问PointPairList，根据自己的需要增加新数据或修改已存在的数据
            IPointListEdit list = line.Points as IPointListEdit;

            //如果list为null，curve.Point不支持该类型

            if(list==null)
            {
                return;
            }

            //得到时间，加入list中的x轴和y轴
            double time = (Environment.TickCount - tickStart) / 1000.0;
            list.Add(time, data);

            //调整x轴的最大值与最小值
            Scale x = zedgraph.GraphPane.XAxis.Scale;
            if(time>x.Max-x.MajorStep)
            {
                x.Max = time + x.MajorStep;
                x.Min = x.Max - 30.0;
            }

            //force a redraw
            zedgraph.AxisChange();

            //调用方法方法更新图表
            zedgraph.Invalidate();
        }
        
        //将led的rgb混合色显示至矩形中
        private void rgb()
        {
            SolidColorBrush xx = new SolidColorBrush(Color.FromRgb((byte)red.Value, (byte)green.Value, (byte)blue.Value));
            led_color.Fill = xx;
        }
        //记录log点击按钮状态
        int click = 0;
        private void Save_Click(object sender,RoutedEventArgs e)
        {
            click = 1;                          
        }

        //异步写日志，不阻碍主线程
        private async void End_Click(object sender, RoutedEventArgs e)
        {
            //检查串口实例是否初始化
            if(port==null)
            {
                return;
            }

            SaveFileDialog save = new SaveFileDialog();
            System.IO.Stream stream;

            //设置文件类型
            save.Filter = "日志|*.json;*.csv;*.xml";
            //设置默认文件类型显示顺序
            save.FilterIndex = 3;
            //保存对话框是否记忆上次打开的目录
            save.RestoreDirectory = true;

            //得到当前时间
            DateTime date = DateTime.Now;
            string day = date.ToShortDateString().ToString().Replace('/', '-');
            string time = date.ToLongTimeString().ToString().Replace(':', '-');
            //设置默认文件名
            save.FileName = string.Format("log-{0}-{1}",
                day, time);
            
            string temp = _log.getString + "port:" + serialPort.SelectedItem.ToString() + "\n" +
                "BPS:" + bps.SelectedItem.ToString() + "}";

            UnicodeEncoding unicode=new UnicodeEncoding();

            byte[] data = unicode.GetBytes(temp);

            //保存当前日志文件
            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if ((stream = save.OpenFile()) != null)
                {
                    await stream.WriteAsync(data, 0, data.Length);
                    stream.Close();
                    
                }
               
            }
        }

        //点击取消按钮日志记录清零
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _log.getString = "";
        }
    }
}
