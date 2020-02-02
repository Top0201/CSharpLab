using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Arduino_MIDI
{

    public class log
    {
        
        private byte[] ad;
        private byte[] pwm;        
        private string s = "{";
        
        public log()
        {

        }
           
        public byte[] AD
        {
            get
            {
                return ad;
            }
            set
            {
                if(ad!=value)
                {
                    ad = value;
                }
            }
        }

        public byte[] PWM
        {
            get
            {
                return pwm;
            }
            set
            {
                if(pwm!=value)
                {
                    pwm = value;
                }
            }
        }
     
        public string getString
        {
            get
            {
                return s;
            }
            set
            {
                if(s!=value)
                {
                    s = value;
                }
            }
        }

        
       public void setString()
       {
            string t1 = "";
            string t2 = "";
            for(int i=0;i<ad.Length;i++)
            {
                if(i==ad.Length-1)
                {
                    t1 += string.Format("{0:X2}",ad[i]);
                }
                else
                {
                    t1 += string.Format("{0:X2}", ad[i]) + " ";
                }
            }

            for(int i=0;i<pwm.Length;i++)
            {
                if(i==pwm.Length-1)
                {
                    t2 += string.Format("{0:X2}", pwm[i]);
                }

                else
                {
                    t2 += string.Format("{0:X2}", pwm[i]) + " ";
                }
            }
            s += "AD:" + t1 + "\n" +
                "pwm:" + t2 + "\n";
       }
    }
}
