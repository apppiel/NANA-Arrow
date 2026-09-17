using System;
using System.IO;

namespace NanaArrow.Core
{
    /// <summary>
    /// 저장 파일 하나의 읽기·쓰기 (NO.3 SaveService 이식). 순수 C# — 경로는 생성자 주입이라 EditMode 테스트는 임시 폴더로 돈다.
    /// 원자적 쓰기: {path}.tmp 에 다 쓴 뒤 바꿔치기 → 쓰는 도중 앱이 죽어도 이전 파일이 살아 있다.
    /// 저장 시점은 호출자가 정한다 — 값이 바뀌는 순간 즉시 (Android 강제 종료는 OnApplicationQuit 이 안 온다).
    /// </summary>
    public sealed class SaveService
    {
        private readonly string _path;
        private readonly string _tmpPath;

        public SaveService(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path must not be empty", nameof(path));
            _path = path;
            _tmpPath = path + ".tmp";
        }

        /// <summary>null = 없음 · 못 읽음 · 손상 · 변조 (구분하지 않음). 호출자는 <see cref="SaveData.Fresh"/> 로 시작한다.</summary>
        public SaveData? Load()
        {
            if (!File.Exists(_path)) return null;

            string text;
            try { text = File.ReadAllText(_path); }
            catch (IOException) { return null; }
            catch (UnauthorizedAccessException) { return null; }

            return SaveCodec.TryDecode(text, out var data) ? data : (SaveData?)null;
        }

        /// <summary>false = 디스크 오류(용량·권한). 로그만 남기고 게임은 계속한다.</summary>
        public bool Save(in SaveData data)
        {
            try
            {
                var dir = Path.GetDirectoryName(_path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                File.WriteAllText(_tmpPath, SaveCodec.Encode(data));

                if (File.Exists(_path)) File.Replace(_tmpPath, _path, null);
                else File.Move(_tmpPath, _path);
                return true;
            }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
        }

        /// <summary>저장 파일 삭제. 없으면 no-op. 테스트·치트용.</summary>
        public void Delete()
        {
            if (File.Exists(_path)) File.Delete(_path);
            if (File.Exists(_tmpPath)) File.Delete(_tmpPath);
        }
    }
}
