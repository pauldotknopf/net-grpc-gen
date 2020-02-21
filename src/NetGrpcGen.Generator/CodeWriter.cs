using System;
using System.IO;

namespace NetGrpcGen.Generator
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

        public IDisposable Indent(bool brackets = false)
        {
            if (brackets)
            {
                WriteLine("{");
            }
            _indentCount++;
            return new ReleaseIndent(this, brackets);
        }

        private class ReleaseIndent : IDisposable
        {
            private readonly CodeWriter _codeWriter;
            private readonly bool _brackets;

            public ReleaseIndent(CodeWriter codeWriter, bool brackets)
            {
                _codeWriter = codeWriter;
                _brackets = brackets;
            }

            public void Dispose()
            {
                _codeWriter._indentCount--;
                if (_brackets)
                {
                    _codeWriter.WriteLine("}");
                }
            }
        }
    }
}