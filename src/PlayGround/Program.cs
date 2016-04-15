using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MzLite.Model;

namespace PlayGround
{
    class Program
    {
        static void Main(string[] args)
        {

            SampleList list = new SampleList(new Sample[] { new Sample("test") });            
            list.Add( new Sample("test1"));
            Sample s = list["test1"];
            list.Rename(s, "test2");
            s = list["test2"];
        }
    }
}
