using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;

namespace Svn2Svn
{
    public class SvnInfo
    {
        SvnClient _client = new SvnClient();
        SvnInfoEventArgs _info = null;

        public SvnInfo(string path)
        {
            _client.Info(SvnTarget.FromString(path), new EventHandler<SvnInfoEventArgs>(OnInfo));
        }

        private void OnInfo(object sender, SvnInfoEventArgs info)
        {
            _info = info;
            info.Detach();
        }

        public SvnInfoEventArgs Info
        {
            get
            {
                return _info;
            }
        }
    }
}
