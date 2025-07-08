using Microsoft.Extensions.Caching.Memory;
using Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Services
{
    public class AccessService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _expiration = TimeSpan.FromHours(2);

        public AccessService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public CodesResponseDto GenerateCodes(int count)
        {
            var rng = new Random();
            string NewCode()
            {
                string c;
                do { c = rng.Next(100000, 999999).ToString(); }
                while (_cache.TryGetValue(c, out _));
                return c;
            }
 
            var studentCode = NewCode();
            _cache.Set(studentCode, "student", new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _expiration
            });

            var examinerCodes = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                var code = NewCode();
                examinerCodes.Add(code);
                _cache.Set(code, "examiner", new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _expiration
                });
            }

            return new CodesResponseDto
            {
                StudentCode = studentCode,
                ExaminerCodes = examinerCodes
            };
        }

        public bool TryValidateCode(string code, out string role)
        {
            if (_cache.TryGetValue(code, out role))
            {
                return true;
            }
            role = null;
            return false;
        }

        public string CreateSession(string role)
        {
            var token = Guid.NewGuid().ToString();
            _cache.Set(token, role, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _expiration
            });
            return token;
        }

        public bool ValidateSession(string token, out string role)
        {
            return _cache.TryGetValue(token, out role);
        }

    }
}
