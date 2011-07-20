﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Security.Authentication;
using System.Data.SqlClient;
using CSGeneral;

public class ApsimBuildsDB
{
    private SqlConnection Connection;
    
    /// <summary>
    /// Open the SoilsDB ready for use.
    /// </summary>
    public void Open()
    {
        //string ConnectionString = "Data Source=www.apsim.info\\SQLEXPRESS;Initial Catalog=\"APSIM Builds\";Integrated Security=True";
        string ConnectionString = "Data Source=www.apsim.info\\SQLEXPRESS;Database=\"APSIM Builds\";Trusted_Connection=True;User ID=apsrunet;password=CsiroDMZ!";
        Connection = new SqlConnection(ConnectionString);
        Connection.Open();
    }

    /// <summary>   
    /// Close the SoilsDB connection
    /// </summary>
    public void Close()
    {
        if (Connection != null)
        {
            Connection.Close();
            Connection = null;
        }
    }

    /// <summary>
    /// Add a new entry to the builds database.
    /// </summary>
    public void Add(string UserName, string Password, string PatchFileName, string Description, int BugID, bool DoCommit)
    {
        string SQL = "INSERT INTO BuildJobs (UserName, Password, PatchFileName, Description, BugID, DoCommit, Status) " +
                     "VALUES (@UserName, @Password, @PatchFileName, @Description, @BugID, @DoCommit, @Status)";

        SqlCommand Cmd = new SqlCommand(SQL, Connection);

        Cmd.Parameters.Add(new SqlParameter("@UserName", UserName));
        Cmd.Parameters.Add(new SqlParameter("@Password", Password));
        Cmd.Parameters.Add(new SqlParameter("@PatchFileName", PatchFileName));
        Cmd.Parameters.Add(new SqlParameter("@Description", Description));
        Cmd.Parameters.Add(new SqlParameter("@BugID", BugID));
        Cmd.Parameters.Add(new SqlParameter("@Status", "Queued"));
        if (DoCommit)
            Cmd.Parameters.Add(new SqlParameter("@DoCommit", "1"));
        else
            Cmd.Parameters.Add(new SqlParameter("@DoCommit", "0"));
        Cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Return details about a specific job.
    /// </summary>
    public Dictionary<string, object> GetDetails(int JobID)
    {
        string SQL = "SELECT * FROM BuildJobs WHERE ID = " + JobID.ToString();

        SqlCommand Command = new SqlCommand(SQL, Connection);
        SqlDataReader Reader = Command.ExecuteReader();
        if (Reader.Read()) 
        {
            Dictionary<string, object> Items = new Dictionary<string, object>();
            for (int i = 0; i < Reader.FieldCount; i++)
            {
                Items.Add(Reader.GetName(i), Reader[i]);
            }
            Reader.Close();
            return Items;
        }
        else
        {
            Reader.Close();
            return null;
        }
    }

    /// <summary>
    /// Update the status of the specified build job.
    /// </summary>
    public void UpdateStatus(int JobID, string NewStatus)
    {
        string SQL = "UPDATE BuildJobs SET Status = '" + NewStatus + "' WHERE ID = " + JobID.ToString();

        SqlCommand Command = new SqlCommand(SQL, Connection);
        Command.ExecuteNonQuery();
    }

    /// <summary>
    /// Update the status of the specified build job.
    /// </summary>
    public void UpdateStartDateToNow(int JobID)
    {
        string NowString = DateTime.Now.ToString("yyyy-MM-dd hh:mm");
        string SQL = "UPDATE BuildJobs SET StartTime = '" + NowString + "' WHERE ID = " + JobID.ToString();

        SqlCommand Command = new SqlCommand(SQL, Connection);
        Command.ExecuteNonQuery();
    }

    /// <summary>
    /// Update the status of the specified build job.
    /// </summary>
    public void UpdateEndDateToNow(int JobID)
    {
        string NowString = DateTime.Now.ToString("yyyy-MM-dd hh:mm");
        string SQL = "UPDATE BuildJobs SET FinishTime = '" + NowString + "' WHERE ID = " + JobID.ToString();

        SqlCommand Command = new SqlCommand(SQL, Connection);
        Command.ExecuteNonQuery();
    }

    /// <summary>
    /// Update the revision number for the specified build job.
    /// </summary>
    public void UpdateRevisionNumber(int JobID, int RevisionNumber)
    {
        string SQL = "UPDATE BuildJobs SET RevisionNumber = " + RevisionNumber.ToString() + " WHERE ID = " + JobID.ToString();

        SqlCommand Command = new SqlCommand(SQL, Connection);
        Command.ExecuteNonQuery();
    }

    /// <summary>
    /// Update the paths for all the revision number for the specified build job.
    /// </summary>
    public void UpdateDiffFileName(int JobID, string DiffsFileName)
    {
        string SQL = "UPDATE BuildJobs SET DiffsFileName = '" + DiffsFileName + "'" +
                                            " WHERE ID = " + JobID.ToString();

        SqlCommand Command = new SqlCommand(SQL, Connection);
        Command.ExecuteNonQuery();
    }

    /// <summary>
    /// Set the number of diffs.
    /// </summary>
    public void SetNumDiffs(int JobID, int NumDiffs)
    {
        string SQL = "UPDATE BuildJobs SET NumDiffs = " + NumDiffs.ToString() + " WHERE ID = " + JobID.ToString();

        SqlCommand Command = new SqlCommand(SQL, Connection);
        Command.ExecuteNonQuery();
    }


    /// <summary>
    /// Updates a field in the database for the specified job with the specified value.
    /// </summary>
    public void UpdateField(int JobID, string FieldName, string FieldValue)
    {
        string SQL = "UPDATE BuildJobs SET " + FieldName + " = '" + FieldValue + "'" +
                                            " WHERE ID = " + JobID.ToString();

        SqlCommand Command = new SqlCommand(SQL, Connection);
        Command.ExecuteNonQuery();
    }


    /// <summary>
    /// Find the next job to run.
    /// </summary>
    public int FindNextJob()
    {
        int JobID = -1;
        string SQL = "SELECT ID FROM BuildJobs WHERE Status = 'Queued' ORDER BY ID";

        SqlCommand Command = new SqlCommand(SQL, Connection);
        SqlDataReader Reader = Command.ExecuteReader();
        if (Reader.Read())
            JobID = Convert.ToInt32(Reader["ID"]);
        Reader.Close();
        return JobID;
    }



    /// <summary>
    /// A dummy method for altering the database structure.
    /// </summary>
    public void Dummy()
    {
        //string SQL = "ALTER TABLE BuildJobs ALTER COLUMN Status char(30)";
        //string SQL = "ALTER TABLE BuildJobs ADD NumDiffs int";
        //string SQL = "ALTER TABLE BuildJobs ADD BuildTreeFileName varchar";
        //string SQL = "ALTER TABLE BuildJobs DROP COLUMN RevisionNumber";
        //string SQL = "DELETE FROM BuildJobs";
        string SQL = "ALTER TABLE BuildJobs ADD DoCommit int";

        SqlCommand Command = new SqlCommand(SQL, Connection);
        Command.ExecuteNonQuery();
    }



}