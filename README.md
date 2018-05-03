# MWCA.Net
.Net implementation of MWCA (https://github.com/emirozturk/MWCAC)  
MWCA.Net could be used as a dll for a project.  
Content of a text file should be read as byte array as shown below:  
            byte[] input = File.ReadAllBytes("sample.txt");    
MWCA.Compress method takes a byte array as an argument and gives MWCAStream object:  
            MWCAStream writeMs = MWCA.Compress(input);  
MWCAStream object includes two methods named WriteMultiStream and WriteSingleStream:  
            writeMs.WriteMultiStream("sample.txt");  
  
To read six streams, first MWCAStream should be created:  
            MWCAStream readMs = new MWCAStream();  
After that, ReadMultiStream method searches for six files appending file extensions to given filename:  
            readMs.ReadMultiStream("sample.txt");  
Decompressed stream could be obtained using MWCA.Decompress:  
            byte[] readBuffer = MWCA.Decompress(readMs);  
