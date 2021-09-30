﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


namespace CCSFileExplorerWV
{
    public class CCSFile
    {
        public enum FileVersionEnum { HACK_GU, IMOQF }

        public FileVersionEnum FileVersion;

        public byte[] raw;
        public bool isvalid;
        public Block0001 header;
        public Block0002 toc;
        public List<FileEntry> files;

        public CCSFile(byte[] rawBuffer, FileVersionEnum FileVersion)
        {
            this.raw = rawBuffer;
            this.FileVersion = FileVersion;
            Reload();
        }

        public void Reload()
        {             
            isvalid = false;
            MemoryStream m = new MemoryStream(raw);
            m.Seek(0, 0);
            List<Block> blocks = new List<Block>();
            while (m.Position < raw.Length)
            {
                Block b = Block.ReadBlock(m);
                b.CCSFile = this;
                blocks.Add(b);
            }
            if (blocks.Count == 0)
                return;
            if (blocks[blocks.Count - 1].BlockID != 0xCCCCFF01 ||
                blocks[blocks.Count - 1].ID != 0xFFFFFFFF)
                return;
            isvalid = true;
            header = (Block0001)blocks[0];
            toc = (Block0002)blocks[1];
            files = new List<FileEntry>();
            for (int i = 0; i < toc.FileCount; i++)
            {
                FileEntry entry = new FileEntry(toc.filenames[i]);
                for (int j = 0; j < toc.ObjCount; j++)
                    if (toc.indexes[j] - 1 == i)
                    {
                        ObjectEntry obj = new ObjectEntry(toc.objnames[j]);
                        for (int k = 2; k < blocks.Count; k++)
                            if (blocks[k].ID - 1 == j)
                            {
                                obj.blocks.Add(blocks[k]);
                                blocks[k].ObjectEntry = obj;
                                blocks[k].FileEntry = entry;
                            }
                        entry.objects.Add(obj);
                    }
                files.Add(entry);
                
            }
        }

        public void Rebuild()
        {
            MemoryStream m = new MemoryStream();
            header.WriteBlock(m);
            toc.WriteBlock(m);
            foreach (FileEntry file in files)
                foreach (ObjectEntry obj in file.objects)
                    foreach (Block b in obj.blocks)
                        b.WriteBlock(m);
            new BlockDefault(0xCCCC0005, 1, new byte[0]).WriteBlock(m);
            new BlockDefault(0xCCCCFF01, 0xFFFFFFFF, new byte[0]).WriteBlock(m);
            raw = m.ToArray();
        }

        public void Save(string filename)
        {
            Rebuild();
            File.WriteAllBytes(filename, raw);
        }

        public string Info()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Arquivo Válido : " + isvalid);
            if (isvalid)
                sb.AppendLine(files.Count + " files loaded");
            return sb.ToString();
        }

        public static Bitmap CreateImage(byte[] palette, byte[] data)
        {
            List<Color> pal = new List<Color>();
            int pos = 0x10;
            byte r, g, b, a;
            while (pos < palette.Length)
            {
                r = palette[pos];
                g = palette[pos + 1];
                b = palette[pos + 2];
                a = palette[pos + 3];
                if (a <= 128)
                    a = (byte)((a * 255) / 128);
                pal.Add(Color.FromArgb(a, r, g, b));
                pos += 4;
            }
            int sizeX = (int)Math.Pow(2, data[0xC]);
            int sizeY = (int)Math.Pow(2, data[0xD]);
            pos = 0x18;
            int dataSize = data.Length - pos;
            Bitmap result = new Bitmap(sizeX, sizeY);
            if (dataSize * 2 == sizeX * sizeY)
                for (int y = 0; y < sizeY; y++)
                    for (int x = 0; x < sizeX / 2; x++)
                    {
                        result.SetPixel(x * 2, sizeY - y - 1, pal[data[pos] % 0x10]);
                        result.SetPixel(x * 2 + 1, sizeY - y - 1, pal[data[pos] / 0x10]);
                        pos++;
                    }
            else if (dataSize >= sizeX * sizeY)
                for (int y = 0; y < sizeY; y++)
                    for (int x = 0; x < sizeX; x++)
                        result.SetPixel(x, sizeY - y - 1, pal[data[pos++]]);
            return result;
        }
    }
}
