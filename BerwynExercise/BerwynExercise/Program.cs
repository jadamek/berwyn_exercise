using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BerwynExercise
{
    class Program
    {
        //-----------------------------------------------------------------------------
        // * Berwyn Programming Exercise
        //-----------------------------------------------------------------------------
        // Process a CSV file input as the first argument to the program, outputting
        // the results to a second CSV
        //-----------------------------------------------------------------------------
        static void Main(string[] args)
        {
            if(args.Length > 0)
            {
                if(System.IO.File.Exists(args[0]))
                {
                    ProcessData(args[0]);
                }
            }
        }        

        //-----------------------------------------------------------------------------
        // * Process CSV Records
        //-----------------------------------------------------------------------------
        // Ingests a CSV file at the specified file path of the format:
        // GUID, VAL1, VAL2, VAL3 (per-line)
        //
        // From this input, produces a result CSV with following format:
        //  COUNT (total records)
        //  GUID, BEST_SUM (of val1 + val2 for a single row)
        //  DUPLICATE_IDS (as a comma-separated list)
        //  AVERAGE_LENGTH_V3
        //  GUID, V1+V2, IS_DUPLICATE, IS_GREATER_THAN_AVERAGE_V3 (per-record)
        //
        // Result filepath is the input path + "-processed", e.g:
        //  filepath    => /home/data/myRecords.csv
        //  output      => /home/data/myRecords-processed.csv
        //-----------------------------------------------------------------------------
        static void ProcessData(string filepath)
        {
            // Setup instream, unique ID set for duplication detection, and an array of
            // all records for later computation of GREATER_THAN_AVERAGE
            System.IO.StreamReader  infile = new System.IO.StreamReader(filepath); 
            HashSet<string>         uniqueGUIDs = new HashSet<string>();
            HashSet<string>         duplicateGUIDs = new HashSet<string>();
            List<BerwynRecord>      processed = new List<BerwynRecord>();
           
            // Counters needed for statistic computations over the record set
            int     recordCount = 0;
            string  maxV1V2Guid = "N/A";
            int     maxV1V2Sum = 0;
            int     val3LengthSum = 0;

            // Read-in each line of the input file as a single record
            string row;

            // Skip CSV Header line
            row = infile.ReadLine();

            while ((row = infile.ReadLine()) != null)
            {

                string[] record = row.Replace("\"", "").Split(',');
                BerwynRecord result = new BerwynRecord();

                // Process GUID
                result.Guid = record[0];

                // Check for duplicated GUID
                if(!uniqueGUIDs.Add(result.Guid))
                {
                    result.IsDuplicateGUID = true;
                    duplicateGUIDs.Add(result.Guid);
                }

                // Process VAL1 + VAL2 sum
                result.V1V2Sum = Int32.Parse(record[1]) + Int32.Parse(record[2]);

                // Check to see if this is a new maximum VAL1 VAL2 sum
                if(result.V1V2Sum > maxV1V2Sum)
                {
                    maxV1V2Sum = result.V1V2Sum;
                    maxV1V2Guid = result.Guid;
                }

                // Process Val3
                result.Val3 = record[3].Length;
                val3LengthSum += result.Val3;

                processed.Add(result);
                recordCount++;
            }

            infile.Close();

            // Compute average VAL3 length
            double val3Average = val3LengthSum;
            val3Average /= recordCount;

            // Output the final processed results
            string outputPath = filepath.Insert(filepath.LastIndexOf('.'), "-processed");
            System.IO.StreamWriter outfile = new System.IO.StreamWriter(outputPath);

            // Output header
            outfile.WriteLine(recordCount);
            outfile.Write(maxV1V2Guid);
            outfile.Write(",");
            outfile.WriteLine(maxV1V2Sum);

            foreach (string duplicateGUID in duplicateGUIDs)
            {
                outfile.Write(duplicateGUID);
                outfile.Write(",");
            }
            outfile.WriteLine();

            outfile.WriteLine(val3Average);

            // Output per-record processed results
            foreach(BerwynRecord result in processed)
            {
                outfile.Write(result.Guid);
                outfile.Write(",");

                outfile.Write(result.V1V2Sum);
                outfile.Write(",");

                outfile.Write((result.IsDuplicateGUID ? "Y" : "N"));
                outfile.Write(",");

                outfile.WriteLine((result.Val3 > val3Average ? "Y" : "N"));
            }

            outfile.Close();
        }

        //=============================================================================
        // ** Berwyn CSV Output Record
        //=============================================================================
        // Simple 4-field class representing a processed record
        //=============================================================================
        class BerwynRecord
        {
            // Fields
            public string Guid;
            public int V1V2Sum;
            public bool IsDuplicateGUID;
            public int Val3;
        }
    }
}
