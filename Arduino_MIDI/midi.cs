using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Arduino_MIDI
{

    //处理arduino发送给pc数据的类
    public class midi:INotifyPropertyChanged
    {
        //接收数据字节数组
        private byte[] receive_buffer;

        //发送数据字节数组，用于处理led的滑动条
        private byte[] send_buffer;

        ////led的通道号
        //private string pin;

        ////led的value值
        //private int state;

        //属性变化事件
        public event PropertyChangedEventHandler PropertyChanged;

        //初始化字节数据数组
        public midi()
        {
            send_buffer = new byte[3];
            send_buffer[0] = 0;
            send_buffer[1] = 0;
            send_buffer[2] = 0;

            //pin = "";
            //state = 0;
            
            receive_buffer = new byte[3];
            receive_buffer[0] = 0;
            receive_buffer[1] = 0;
            receive_buffer[2] = 0;

            
        }

        //?
        private void NotifyPropertyChanged([CallerMemberName] String propertyName="")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs( propertyName));
        }

        //得到接收数据字节数组
        public byte[] getReceive
        {
            get
            {
                return receive_buffer;
            }
            set
            {
                receive_buffer = value;
                NotifyPropertyChanged();
            }
        }

        //得到发送数据字节数组
        public byte[] getSend
        {
            get
            {
                return send_buffer;
            }
            set
            {
                if(send_buffer!=value)
                {
                    send_buffer = value;
                }
                //send_buffer[0] = (byte)(0xd0 | int.Parse(pin));
                //send_buffer[1] = (byte)(state & 0x7f);
                //send_buffer[2] = (byte)((state >> 7) & 0x7f);
            }
        }

        ////绑定至slider的tag
        //public string Pin
        //{
        //    get
        //    {
        //        return pin;
        //    }
        //    set
        //    {
        //        if(pin!=value)
        //        {
        //            pin = value;
        //            NotifyPropertyChanged();
        //        }
        //    }
        //}
        
        //绑定至slider的value
        //public int State
        //{
        //    get
        //    {
        //        return state;
        //    }
        //    set
        //    {
        //        if(state!=value)
        //        {
        //            state = value;
        //            NotifyPropertyChanged();
        //        }
        //    }

        //}

        public int index { get; set; }

        //利用接收数据字节数组将数据还原
        public int byte_to_int
        {
            get
            {
                return ((int)receive_buffer[2] << 7) + receive_buffer[1];
            }

        }

        //pc接收arduino发送数据
        private string receiveMessage;
        public string ReceiveMesssage
        {
            get
            {
                return receiveMessage;
            }
            set
            {
                if(receiveMessage!=value)
                {
                    receiveMessage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        //pc发送给arduino的数据
        private string sendMessage;
        public string SendMessage
        {
            get
            {
                return sendMessage;
            }
            set
            {
                if(sendMessage!=value)
                {
                    sendMessage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public byte[] SendMessageToByte
        {
            get
            {
                if(string.IsNullOrEmpty(sendMessage))
                {
                    return null;
                }

                string[] temp = sendMessage.Split(new Char[] { ' ', ',', '.', ':', '\t' });
                byte[] result = new byte[temp.Length];

                for(int i=0;i<temp.Length;i++)
                {
                    //转换不成功的异常处理
                    if((byte.TryParse(temp[i],System.Globalization.NumberStyles.HexNumber,
                        null,out result[i]))==false)
                    {
                        result[i] = 0;
                    }
                }

                return result;
            }
        }

        //温度
        private int degree;
        public int getDegree
        {
            get
            {
                return degree;
            }
            set
            {
                if(degree!=value)
                {
                    degree = value;
                    NotifyPropertyChanged();
                }
            }
        }

        //光强
        private int light_intense;
        public int getLight
        {
            get
            {
                return light_intense;
            }
            set
            {
                if(light_intense!=value)
                {
                    light_intense = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
