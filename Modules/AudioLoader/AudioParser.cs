using Godot;
using System;
using System.Collections.Generic;

public partial class AudioParser
{
    private static void ReportErrors(Error err, string filepath)
    {
        // List of possible errors when working with files
        var resultHash = new Dictionary<Error, string>
        {
            { Error.FileNotFound, "File: not found" },
            { Error.FileBadDrive, "File: Bad drive error" },
            { Error.FileBadPath, "File: Bad path error." },
            { Error.FileNoPermission, "File: No permission error." },
            { Error.FileAlreadyInUse, "File: Already in use error." },
            { Error.FileCantOpen, "File: Can't open error." },
            { Error.FileCantWrite, "File: Can't write error." },
            { Error.FileCantRead, "File: Can't read error." },
            { Error.FileUnrecognized, "File: Unrecognized error." },
            { Error.FileCorrupt, "File: Corrupt error." },
            { Error.FileMissingDependencies, "File: Missing dependencies error." },
            { Error.FileEof, "File: End of file (EOF) error." }
        };

        if (resultHash.ContainsKey(err))
        {
            GD.Print($"Error: {resultHash[err]} {filepath}");
        }
        else
        {
            GD.Print($"Unknown error with file {filepath}, error code: {err}");
        }
    }

    public static AudioStream LoadFile(string filepath)
    {

        var file = FileAccess.Open(filepath, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            ReportErrors(Error.FileCantOpen, filepath);
            return new AudioStreamWav();
        }

        var bytes = file.GetBuffer((long)file.GetLength());

        if (filepath.EndsWith(".wav"))
        {
            var newStream = new AudioStreamWav();

            int bitsPerSample = 0;
            int i = 0;

            while (true)
            {
                if (i >= bytes.Length - 4)
                {
                    GD.Print("Data byte not found");
                    break;
                }

                string those4bytes = $"{(char)bytes[i]}{(char)bytes[i + 1]}{(char)bytes[i + 2]}{(char)bytes[i + 3]}";

                if (those4bytes == "RIFF")
                {
                    GD.Print($"RIFF OK at bytes {i}-{i + 3}");
                }
                else if (those4bytes == "WAVE")
                {
                    GD.Print($"WAVE OK at bytes {i}-{i + 3}");
                }
                else if (those4bytes == "fmt ")
                {
                    GD.Print($"fmt OK at bytes {i}-{i + 3}");

                    // Getting the size of the subordinate format block
                    int formatSubchunkSize = BitConverter.ToInt32(bytes, i + 4);
                    GD.Print($"Format subchunk size: {formatSubchunkSize}");

                    int fsc0 = i + 8; // The beginning of the subordinate block

                    // Format code [bytes 0-1]
                    int formatCode = BitConverter.ToInt16(bytes, fsc0);
                    string formatName;
                    if (formatCode == 0) formatName = "8_BITS";
                    else if (formatCode == 1) formatName = "16_BITS";
                    else if (formatCode == 2) formatName = "IMA_ADPCM";
                    else
                    {
                        formatName = "UNKNOWN (trying to interpret as 16_BITS)";
                        formatCode = 1;
                    }
                    GD.Print($"Format: {formatCode} {formatName}");
                    newStream.Format = (AudioStreamWav.FormatEnum)formatCode;

                    // Number of channels [bytes 2-3]
                    int channelNum = BitConverter.ToInt16(bytes, fsc0 + 2);
                    GD.Print($"Number of channels: {channelNum}");
                    newStream.Stereo = channelNum == 2;

                    // Sampling rate [bytes 4-7]
                    int sampleRate = BitConverter.ToInt32(bytes, fsc0 + 4);
                    GD.Print($"Sample rate: {sampleRate}");
                    newStream.MixRate = sampleRate;

                    // Bytrate [bytes 8-11]
                    int byteRate = BitConverter.ToInt32(bytes, fsc0 + 8);
                    GD.Print($"Byte rate: {byteRate}");

                    // BitsPerSample * Channel / 8 [bytes 12-13]
                    int bitsSampleChannel = BitConverter.ToInt16(bytes, fsc0 + 12);
                    GD.Print($"BitsPerSample * Channel / 8: {bitsSampleChannel}");

                    // Bits per sample [bytes 14-15]
                    bitsPerSample = BitConverter.ToInt16(bytes, fsc0 + 14);
                    GD.Print($"Bits per sample: {bitsPerSample}");
                }
                else if (those4bytes == "data")
                {
                    if (bitsPerSample == 0)
                    {
                        GD.PrintErr("Bits per sample not found before data chunk.");
                        break;
                    }

                    int audioDataSize = BitConverter.ToInt32(bytes, i + 4);
                    GD.Print($"Audio data/stream size is {audioDataSize} bytes");

                    int dataEntryPoint = i + 8;
                    GD.Print($"Audio data starts at byte {dataEntryPoint}");

                    byte[] data = new byte[audioDataSize];
                    Array.Copy(bytes, dataEntryPoint, data, 0, audioDataSize);

                    if (bitsPerSample == 24 || bitsPerSample == 32)
                    {
                        newStream.Data = ConvertTo16Bit(data, bitsPerSample);
                    }
                    else
                    {
                        newStream.Data = data;
                    }
                    break;
                }
                i += 1;
            }

            // Setting the cycle parameters
            int sampleNum = newStream.Data.Length / 4;
            newStream.LoopEnd = sampleNum;
            newStream.LoopMode = AudioStreamWav.LoopModeEnum.Forward;

            return newStream;
        }
        else if (filepath.EndsWith(".ogg"))
        {
            var newStream = ResourceLoader.Load<AudioStream>(filepath);

            return newStream;
        }
        else if (filepath.EndsWith(".mp3"))
        {
            var newStream = new AudioStreamMP3
            {
                Loop = true,
                Data = bytes
            };
            return newStream;
        }
        else
        {
            GD.Print("ERROR: Wrong filetype or format");
        }
        file.Close();
        return null;


    }

    // Method for converting WAV data from 24 or 32 bits to 16 bits
    private static byte[] ConvertTo16Bit(byte[] data, int bitsPerSample)
    {
        GD.Print($"Converting to 16-bit from {bitsPerSample}");
        double startTime = Time.GetTicksMsec() / 1000.0;

        if (bitsPerSample == 24)
        {
            int newSize = data.Length * 2 / 3;
            byte[] newData = new byte[newSize];
            int j = 0;
            for (int i = 0; i < data.Length; i += 3)
            {
                newData[j] = data[i + 1];
                newData[j + 1] = data[i + 2];
                j += 2;
            }
            GD.Print($"Took {Time.GetTicksMsec() / 1000.0 - startTime} seconds for conversion");
            return newData;
        }
        else if (bitsPerSample == 32)
        {
            int newSize = data.Length / 2;
            byte[] newData = new byte[newSize];
            for (int i = 0; i < data.Length; i += 4)
            {
                float singleFloat = BitConverter.ToSingle(data, i);
                int value = (int)(singleFloat * 32768);
                int j = i / 2;
                newData[j] = (byte)(value & 0xFF);
                newData[j + 1] = (byte)((value >> 8) & 0xFF);
            }
            GD.Print($"Took {Time.GetTicksMsec() / 1000.0 - startTime} seconds for conversion");
            return newData;
        }
        else
        {
            return data;
        }
    }
}
