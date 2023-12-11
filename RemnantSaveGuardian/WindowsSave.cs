using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RemnantSaveGuardian
{
    class WindowsSave
    {
        public string Container { get; set; }
        public string Profile { get; set; }
        public List<string> Worlds { get; set; }
        private bool isValid;
        public bool Valid { get { return isValid; } }

        //profile
        private byte[] ProfileFlag = new byte[] { 0x70, 0x00, 0x72, 0x00, 0x6f, 0x00, 0x66, 0x00, 0x69, 0x00, 0x6c, 0x00, 0x65, 0x00 };
        //save_
        private byte[] WorldFlag = new byte[] { 0x73, 0x00, 0x61, 0x00, 0x76, 0x00, 0x65, 0x00, 0x5F, 0x00 };


        public WindowsSave(string containerPath)
        {
            Worlds = new List<string>();
            Container = containerPath;
            var folderPath = new FileInfo(containerPath).Directory.FullName;
            byte[] byteBuffer = File.ReadAllBytes(Container);

            //find profile in byte
            var index = IndexOf(byteBuffer, ProfileFlag).LastOrDefault();

            var offset = index + 61;

            var profileFolderNameBytes = new byte[16];
            Array.Copy(byteBuffer, offset, profileFolderNameBytes, 0, 16);
            var profileGuid = new Guid(profileFolderNameBytes);
            var profileFolderName = profileGuid.ToString().ToUpper().Replace("-", "");
            if (Directory.Exists($@"{folderPath}\{profileFolderName}"))
            {
                var winFiles = Directory.GetFiles($@"{folderPath}\{profileFolderName}", "container.*");
                if (winFiles.Length > 0)
                {
                    GetProfile(winFiles[0], profileFolderName);
                }
            }

            //find word in byte
            var wordIndexs = IndexOf(byteBuffer, WorldFlag);
            var x = 1;
            offset = 59;
            foreach (var wordIndex in wordIndexs)
            {
                if (x % 2 == 0)
                {
                    var wordFolderNameBytes = new byte[16];
                    Array.Copy(byteBuffer, offset+ wordIndex, wordFolderNameBytes, 0, 16);
                    var wordGuid = new Guid(wordFolderNameBytes);
                    var wordFolderName = wordGuid.ToString().ToUpper().Replace("-", "");
                    if (Directory.Exists($@"{folderPath}\{wordFolderName}"))
                    {
                        var winFiles = Directory.GetFiles($@"{folderPath}\{wordFolderName}", "container.*");
                        if (winFiles.Length > 0)
                        {
                            Worlds.Add(GetWordPath(winFiles[0]));
                        }
                    }
                }
                x++;
            }
        }

        public String GetWordPath(string containerPath) 
        {
            var folderPath = new FileInfo(containerPath).Directory.FullName;
            var offset = 136;
            byte[] byteBuffer = File.ReadAllBytes(containerPath);
            var profileBytes = new byte[16];
            Array.Copy(byteBuffer, offset, profileBytes, 0, 16);
            var profileGuid = new Guid(profileBytes);
            return folderPath + "\\" + profileGuid.ToString().ToUpper().Replace("-", "");
        }

        public void GetProfile(string containerPath, String parent)
        {
            var folderPath = new FileInfo(containerPath).Directory.FullName;
            var offset = 136;
            byte[] byteBuffer = File.ReadAllBytes(containerPath);
            var profileBytes = new byte[16];
            Array.Copy(byteBuffer, offset, profileBytes, 0, 16);
            var profileGuid = new Guid(profileBytes);
            Profile = parent + "\\" + profileGuid.ToString().ToUpper().Replace("-", "");
            isValid = File.Exists($@"{folderPath}\{Profile}");
        }

        public IEnumerable<int> IndexOf(byte[] source, byte[] pattern)
        {
            if (IsEmptyLocate(source, pattern))
            {
                yield break;
            }
            for (int i = 0; i < source.Length - pattern.Length; i++)
            {
                if (!IsMatch(source, i, pattern))
                {
                    continue;
                }
                yield return i;
            }
        }

        private bool IsMatch(byte[] array, int position, byte[] candidate)
        {
            if (candidate.Length > (array.Length - position))
            {
                return false;
            }
            for (int i = 0; i < candidate.Length; i++)
            {
                if (array[position + i] != candidate[i])
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsEmptyLocate(byte[] array, byte[] candidate)
        {
            return array == null
                   || candidate == null
                   || array.Length == 0
                   || candidate.Length == 0
                   || candidate.Length > array.Length;
        }
    }
}
