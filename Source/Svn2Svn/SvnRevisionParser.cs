using System;
using SharpSvn;

namespace Svn2Svn
{
    abstract class SvnRevisionParser
    {
        public static SvnRevision Parse(string revision)
        {
            String s = revision.ToLower();
            switch(s)
            {
                case "head":
                    return SvnRevision.Head;
                case "base":
                    return SvnRevision.Base;
                case "none":
                    return SvnRevision.None;
                case "one":
                    return SvnRevision.One;
                default:
                    long i = 0;
                    if (! long.TryParse(s, out i) || i < 0) 
                    {
                        throw new Exception(string.Format("Invalid revision '{0}', must be a positive number, ONE, HEAD, etc.", 
                            revision));
                    }
                    return new SvnRevision(i);
            }            
        }
    }
}
