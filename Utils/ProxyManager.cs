using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace UltimateParser.Utils 
{
    public class ProxyManager 
    {
        private readonly string[] _proxies;
        private int _currentIndex = -1;

        public ProxyManager(IEnumerable<string> proxies)
        {
            _proxies = proxies?.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray() ?? Array.Empty<string>();
        }

        public string? GetNextProxy()
        {
            if (_proxies.Length == 0) return null;
            int index = Interlocked.Increment(ref _currentIndex);
            return _proxies[Math.Abs(index) % _proxies.Length];
        }
    }
}