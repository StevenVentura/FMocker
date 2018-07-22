using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMocker
{
    class FClip
    {
        //record,save,listen,play

        public byte[] data;
        public string fileName;

        public FClip(byte[] data, string fileName)
        {
            this.data = data;
            this.fileName = fileName;
        }


    }
}
