using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Mercury.App
{
    public class CatalogItem : IDisposable
    {
        String _path;
        MemoryStream _iconStream;

        internal CatalogItem(String path)
        {
            _path = path;
            Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("Mercury.App.TestIcon.png");

            SetIconStream(str);
            str.Close();
        }

        public String Title { get { return Path.GetFileName(_path); } }
        public MemoryStream IconStream { get { return _iconStream; } }

        #region IDisposable Members

        public void Dispose()
        {
            if (_iconStream != null)
            {
                _iconStream.Dispose();
                _iconStream = null;
            }
        }

        #endregion

        private void SetIconStream(Stream str)
        {
            _iconStream = new MemoryStream((int)str.Length);

            byte[] block = new byte[4096];
            while (_iconStream.Length < str.Length)
            {
                int bytesRead = str.Read(block, 0, block.Length);
                _iconStream.Write(block, 0, bytesRead);
            }

            _iconStream.Seek(0, SeekOrigin.Begin);
        }
    }   
}
