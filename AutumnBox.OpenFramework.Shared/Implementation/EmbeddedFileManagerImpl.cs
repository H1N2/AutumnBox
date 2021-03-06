﻿/*************************************************
** auth： zsh2401@163.com
** date:  2018/8/2 2:09:45 (UTC +8:00)
** desc： ...
*************************************************/
using AutumnBox.OpenFramework.Open;
using System.IO;

namespace AutumnBox.OpenFramework.Implementation
{
    class EmbeddedFileManagerImpl : IEmbeddedFileManager
    {
        private class EmbeddedFileImpl : IEmbeddedFile
        {
            public readonly object requester;
            public readonly string path;
            public EmbeddedFileImpl(object requester, string path)
            {
                this.requester = requester;
                this.path = path;
            }
            public void CopyTo(Stream targetStream)
            {
                using (var stream = GetStream())
                {
                    stream.CopyTo(targetStream);
                }
            }
            public void ExtractTo(FileInfo targetFile)
            {
                using (FileStream fs = new FileStream(targetFile.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    WriteTo(fs);
                }
            }
            public Stream GetStream()
            {
                string fullPath = requester.GetType().Assembly.GetName().Name + "." + path;
                var stream = requester.GetType().Assembly
                    .GetManifestResourceStream(fullPath);
                return stream;
            }
            public void WriteTo(FileStream fs)
            {
                CopyTo(fs);
            }
        }
        public IEmbeddedFile Get(object context, string innerResPath)
        {
            return new EmbeddedFileImpl(context, innerResPath);
        }
    }
}
