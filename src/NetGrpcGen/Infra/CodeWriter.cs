using System;
using System.IO;

namespace NetGrpcGen.Infra
{
    public class CodeWriter
    {
        private readonly TextWriter _wrapping;
        private int _indentCount = 0;
        
        public CodeWriter(TextWriter wrapping)
        {
            _wrapping = wrapping;
        }
        
        public void WriteLine(string line)
        {
            for (var x = 0; x < _indentCount; x++)
            {
                _wrapping.Write("\t");
            }
            _wrapping.WriteLine(line);
        }

        public void WriteLineIndented(string line)
        {
            using (Indent())
            {
                WriteLine(line);
            }
        }

        public IDisposable Indent()
        {
            _indentCount++;
            return new ReleaseIndent(this);
        }

        private class ReleaseIndent : IDisposable
        {
            private readonly CodeWriter _codeWriter;

            public ReleaseIndent(CodeWriter codeWriter)
            {
                _codeWriter = codeWriter;
            }

            public void Dispose()
            {
                _codeWriter._indentCount--;
            }
        }
    }
}