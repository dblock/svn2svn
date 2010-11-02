/*
 * ======================
 *   Bram's FolderSync
 * ======================
 * http://www.codeproject.com/KB/files/kratfoldersync.aspx
 * kratchkov@inbox.lv
 * 
 * Description:
 *		These classes can be used to compare and to synchronize two
 *		folders by defining simple rules (very basic).
 * 
 * This is the first release.
 * 
 * The FolderDiff class gets differencies between two directories.
 * It raises a state for each file or folder:
 * Possible states are described in enum FolderSynchronisation.ComparisonResult
 * 
 * The FolderSync class uses the FolderDiff and reacts to its events to handle
 * file copying, moving or whatsoever.
 * Possible actions are desctibed in enum FolderSynchronisation.FileActions
 * 
 * btw: If you sense missing 'z' in the comments, don't panic. It's alright, my
 * keyboard has a little something against me writing 'z'...
 * 
 * DISCLAIMER:
 * THIS CODE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY
 * KIND, EITHER EXPRESSED OR IMPLIED INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE.  THE  ENTIRE RISK  AS TO THE  QUALITY AND PERFORMANCE OF THIS
 * CODE IS WITH YOU. SHOULD THIS CODE PROVE DEFECTIVE, YOU ASSUME
 * THE COST OF ALL NECESSARY SERVICING, REPAIR OR CORRECTION .
 * You take full responsibility for the use of this code and any
 * consequences thereof. I can not accept liability for damages
 * or failures arising from the use of this code, or parts of this code.
 * 
 */
using System;
using System.IO;

namespace FolderSynchronisation
{
	/// <summary>
	/// Describes the comparison results between scanned files and directories.
	/// </summary>
	public enum ComparisonResult
	{
		/// <summary>
		/// The two files are identical. That means, they have the same size. There
		/// is nothing such as a CRC check.
		/// </summary>
		Identical = 0,
		/// <summary>
		/// The file is present in the second folder but is missing in the first one.
		/// This is also raised for missing folders.
		/// </summary>
		MissingInFolder1,
		/// <summary>
		/// The file is present in the first folder but is missing in the second one.
		/// This is also raised for missing folders.
		/// </summary>
		MissingInFolder2,
		/// <summary>
		/// The two files have different sizes.
		/// </summary>
		SizeDifferent
	}

	/// <summary>
	/// This delegate describes the event raised from comparing a file.
	/// </summary>
	public delegate void CompareDelegate(ComparisonResult comparisonResult, 
										 FileSystemInfo[] files, bool isAFolder);
	
	/// <summary>
	/// Gets differencies between two folders.
	/// </summary>
	public class FolderDiff 
	{
		/// <summary>
		/// Gets the first foldername.
		/// </summary>
		public string FolderName1 
		{			
			get
			{
				if (baseFolderInfo[1] == null)
					return null;
				return baseFolderInfo[1].FullName;
			}
		}

		/// <summary>
		/// Gets the second foldername.
		/// </summary>
		public string FolderName2 
		{			
			get
			{
				if (baseFolderInfo[2] == null)
					return null;
				return baseFolderInfo[2].FullName;
			}
		}

		/// <summary>
		/// This event is raised for each compared file or directory
		/// </summary>
		public event CompareDelegate CompareEvent;

		private DirectoryInfo[] baseFolderInfo = new DirectoryInfo[2]; // The two base folders
		private bool stop = false; // When set to true, the class stops all activity.

		/// <summary>
		/// Creates a new instance of a FolderDiff which can compare two folders.
		/// </summary>
		/// <param name="folderName1">Path to the first folder.</param>
		/// <param name="folderName2">Path to the second folder.</param>
		public FolderDiff(string folderName1, string folderName2)
		{
			folderName1 = folderName1.TrimEnd('\\');
			folderName2 = folderName2.TrimEnd('\\');

			baseFolderInfo[0] = new DirectoryInfo(folderName1);
			baseFolderInfo[1] = new DirectoryInfo(folderName2);
		}

		/// <summary>
		/// Starts comparing (This may take some time...)
		/// </summary>
		public void Compare()
		{
			if (this.CompareEvent == null)
				return;

			this.stop = false;

									// Compares the dirs
			CompareDirs("\\", 0);   // Looks for missing dirs in dir 2
			CompareDirs("\\", 1);   //   "	"	"	"	"	" in dir 1

			Compare2(@"\");			// Compares the files
		}

		/// <summary>
		/// It compares the two folders and calls itself to recurse.
		/// </summary>
		/// <param name="relativeFolderName">where to start from</param>
		private void Compare2(string relativeFolderName)
		{
			DirectoryInfo[] folderInfo = new DirectoryInfo[2];
			folderInfo[0] = new DirectoryInfo(baseFolderInfo[0].FullName + relativeFolderName);
			folderInfo[1] = new DirectoryInfo(baseFolderInfo[1].FullName + relativeFolderName);

			bool fileCompReturn = CompareFiles(folderInfo); // Compares the files
			if (!fileCompReturn)
				return;

			DirectoryInfo[] subDirs = folderInfo[0].GetDirectories();
			foreach (DirectoryInfo subDir in subDirs)	// Recursion
			{
				if (this.stop)
					return;
				string newRelativeFolderName = relativeFolderName + @"\" + subDir.Name; 
				Compare2(newRelativeFolderName);
			}
		}

		/// <summary>
		/// This cancels all activity immediately.
		/// </summary>
		public void CancelNow()
		{
			this.stop = true;
		}

		/// <summary>
		/// Compares the files in the two folders described by folderInfo[]
		/// </summary>
		private bool CompareFiles(DirectoryInfo[] folderInfo)
		{
			FileInfo[][] filesInFolders = new FileInfo[2][];
			try
			{
				filesInFolders[0] = folderInfo[0].GetFiles();
				filesInFolders[1] = folderInfo[1].GetFiles();
			}
			catch (DirectoryNotFoundException)
			{
				return false; // We won't do anything here because missing folders are handled
							  // by FolderDiff.CompareDirs(string, int)
			}

			for (int i = 0; i < 2; i++)
			{
				foreach (FileInfo thisDirFile in filesInFolders[i])
				{
					if (this.stop)
						return true;
					int otherIndex = Math.Abs(i - 1); // Returns 0 if i == 1; returns 1 if i == 0
					FileInfo otherDirFile = new FileInfo(folderInfo[otherIndex].FullName + @"\" + thisDirFile.Name);
					FileInfo[] files = new FileInfo[2];
					
					// CompareEvent must return as first files index the file in the first
					// folder, that's why I need to set the index of files with i.

					files[i] = thisDirFile;				// thisDirFile is the first if i == 0
					files[otherIndex] = otherDirFile;	// otherDirFile is the second if i == 0

					/*
					if (i == 0)
					{
						files[0] = thisDirFile;
						files[1] = otherDirFile;
					}									// Ugly, but working.
					else								// The former code is nicer, and works too.
					{
						files[0] = otherDirFile;
						files[1] = thisDirFile;
					}*/
					
					if (otherDirFile.Exists)			// The file exists in both dirs
					{
						if (i == 0)
							continue;
						if (otherDirFile.Length == thisDirFile.Length)	
						{
												// both files have same length
												// they are considered identical even though
												// they might be different.
							this.CompareEvent(ComparisonResult.Identical, files, false);
							continue;
						}
						else					// the files have different sizes, they are
						{						// different
							this.CompareEvent(ComparisonResult.SizeDifferent, files, false);
							continue;
						}
					}
					else // The file does not exist in other dir
					{
						if (i == 0)  // i is the index in which the file exists
						{
							this.CompareEvent(ComparisonResult.MissingInFolder2, files, false);
							continue;
						}
						else 
						{
							this.CompareEvent(ComparisonResult.MissingInFolder1, files, false);
							continue;
						}
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Scans directories only. Recursive. It's divided into two parts, one for each base dir.
		/// As it only sees missing dirs in one folder, it has to be called twice. That's why the
		/// int directoryIndexToScan.
		/// </summary>
		/// <param name="relativeFolder">Where to begin (normally \). Used for recursion.</param>
		/// <param name="directoryIndexToScan">Dir index, begins at 0</param>
		private void CompareDirs(string relativeFolder, int directoryIndexToScan)
		{
			DirectoryInfo[] baseDirs = new DirectoryInfo[2];
			baseDirs[0] = new DirectoryInfo(this.baseFolderInfo[0].FullName + relativeFolder);
			baseDirs[1] = new DirectoryInfo(this.baseFolderInfo[1].FullName + relativeFolder);
			
			int otherDirIndex = Math.Abs(directoryIndexToScan - 1);

			foreach (DirectoryInfo subDir in baseDirs[directoryIndexToScan].GetDirectories())
			{
				if (this.stop)
					return;
				DirectoryInfo[] bothDirs = new DirectoryInfo[2];// This'll contain the info sent
																// by the event.
				bothDirs[directoryIndexToScan] = subDir;
				bothDirs[otherDirIndex] = new DirectoryInfo
					(this.baseFolderInfo[otherDirIndex].FullName
					+ relativeFolder + @"\" + subDir.Name);

				if (!bothDirs[otherDirIndex].Exists)
				{
					if (directoryIndexToScan == 0)
						this.CompareEvent(ComparisonResult.MissingInFolder2, bothDirs, true);
					else if (directoryIndexToScan == 1)
						this.CompareEvent(ComparisonResult.MissingInFolder1, bothDirs, true);
				}
				else
					CompareDirs(relativeFolder + @"\" + subDir.Name, directoryIndexToScan);
									// calls itself to recurse
			}
		}
	}

	/// <summary>
	/// Describes the possible actions that may be taken after a compare.
	/// </summary>
	public enum FileActions
	{
		/// <summary>
		/// This cancels the copy. It's like CancelAll()
		/// May only be used as an answer to the AskWhatToDo event.
		/// </summary>
		CancelCopy = 0,
		/// <summary>
		/// When used as defaultaction, it sends an AskWhatToDo event.
		/// </summary>
		Ask,
		/// <summary>
		/// Nothing has to be done.
		/// </summary>
		Ignore,
		/// <summary>
		/// The file must be copied.
		/// May only be used for missing files.
		/// </summary>
		Copy,
		/// <summary>
		/// The file must be deleted (!! NO CONFIRMATION !!).
		/// May only be used for missing files.
		/// </summary>
		Delete,
		/// <summary>
		/// The file with the older modification date must be overwritten.
		/// May only be used for different size files.
		/// </summary>
		OverwriteOlder,
		/// <summary>
		/// The file with the newer modification date must be overwritten.
		/// May only be used for different size files.
		/// </summary>
		OverwriteNewer,
		/// <summary>
		/// The file must be copied from folder 1 to folder 2.
		/// May not be used as default action or as AskWhatToDo event answer for folder
		/// with index 1. (Duh, missing files can't be copied)
		/// </summary>
		Write1to2,
		/// <summary>
		/// The file must be copied from folder 2 to folder 1.
		/// May not be used as default action or as AskWhatToDo event answer for folder
		/// with index 2. (Duh, missing files can't be copied)
		/// </summary>
		Write2to1		
	}

	
	/// <summary>
	/// This describes the event used when the user must be asked what to do.
	/// </summary>
	public delegate FileActions AskWhatToDoDelegate(FileSystemInfo[] files,
													bool isADir, int missingIndex);
	
	/// <summary>
	/// This is raised when an error occurs.
	/// </summary>
	public delegate void ErrorDelegate(Exception e, string[] files);

	/// <summary>
	/// Synchronizes two folders.
	/// </summary>
	public class FolderSync 
	{
		private FolderDiff diff;			// Used to get the differencies
		private string folderName1;
		private string folderName2;
		private FileActions defMissing1;	// default action for files missing in folder 1
		private FileActions defMissing2;	//    "		 "	   "    "      "     "   "    2
		private FileActions defSize;		//    "		 "	   "    "   with different sizes
		private bool initialized = true;

		public event AskWhatToDoDelegate AskWhatToDo;
		public event ErrorDelegate ErrorEvent;			// If this is raised, then you have a problem

		/// <summary>
		/// Initializes a new instance of the FolderSync class which uses FolderDiff to
		/// synchronize two folders. Call Sync() to start synchronizing.
		/// </summary>
		/// <param name="folderName1">First folder name</param>
		/// <param name="folderName2">Second folder name</param>
		/// <param name="defMissingInFolder1">Default action for files which are missing in the first folder.</param>
		/// <param name="defMissingInFolder2">Default action for files which are missing in the second folder.</param>
		/// <param name="defDifferentFiles">Default action for files which have different sizes.</param>
		public FolderSync(string folderName1, string folderName2, 
			FileActions defMissingInFolder1, 
			FileActions defMissingInFolder2,
			FileActions defDifferentFiles)
		{
			if (defMissingInFolder1 == FileActions.OverwriteNewer |
				defMissingInFolder1 == FileActions.OverwriteOlder |
				defMissingInFolder1 == FileActions.Write1to2 |		// These choices are not valid
				defMissingInFolder1 == FileActions.Write2to1)
			{
				this.initialized = false;
				if (this.ErrorEvent != null)
					this.ErrorEvent(new ArgumentException("defaultActionForMissingFiles1 is not correct"), null);
			}
			if (defMissingInFolder2 == FileActions.OverwriteNewer |
				defMissingInFolder2 == FileActions.OverwriteOlder |
				defMissingInFolder2 == FileActions.Write1to2 |		// These choices are not valid
				defMissingInFolder2 == FileActions.Write2to1)
			{
				this.initialized = false;
				if (this.ErrorEvent != null)
					this.ErrorEvent(new ArgumentException("defaultActionForMissingFiles2 is not correct"), null);
			}

			if (defDifferentFiles == FileActions.Copy)
			{
				this.initialized = false;
				if (this.ErrorEvent != null)
					this.ErrorEvent(new ArgumentException("defaultActionForDifferentFiles is not correct"), null);
			}

			this.defMissing1 = defMissingInFolder1;
			this.defMissing2 = defMissingInFolder2;
			this.defSize = defDifferentFiles;
			this.folderName1 = folderName1;
			this.folderName2 = folderName2;
			this.diff = new FolderDiff(this.folderName1, this.folderName2);
			this.diff.CompareEvent += new CompareDelegate(Compared);
		}

		/// <summary>
		/// Cancels all activity now, immediately, without waiting a femtosecond (Without exageration).
		/// </summary>
		public void CancelNow()
		{
			diff.CancelNow(); 
		}

		// This is called by diff.CompareEvent
		private void Compared(ComparisonResult result, FileSystemInfo[] files, bool isADir)
		{
			#region SizeDifferent
			if (result == ComparisonResult.SizeDifferent)  // Can only be files, not dirs
			{
				FileActions action = this.defSize;
				if (action == FileActions.Ask)
					action = this.AskWhatToDo(files, isADir, -1);	// Need to ask user if 
																	// default action is Ask

				if (action == FileActions.CancelCopy)	// user choice: Cancel
				{
					this.CancelNow();
					return;
				}

				switch (action)
				{					
					case FileActions.Ignore:	// Do nothing
						break;

					case FileActions.Delete:	// The file(s) must be deleted
						if (files[0].Exists)
						{
							files[0].Delete();
						}
						else
						{
							files[1].Delete();
						}
						break;

						// Copy by looking at the dates *************************************
					case FileActions.OverwriteNewer:										//	D
						this.OverWriteDate((FileInfo)files[0], (FileInfo)files[1], false);	//	
						break;																//	A
																							//	
					case FileActions.OverwriteOlder:										//	T
						this.OverWriteDate((FileInfo)files[0], (FileInfo)files[1], true);	//	
						break;																//	E
																							//	
						//********************************************************************
					case FileActions.Write1to2:	// Writes the file to folder 2 (the file should exist)
						try
						{
							((FileInfo)files[0]).CopyTo(files[1].FullName, true);
						}
						catch (Exception e) // This should NOT happen (and it doesn't, usually...) 
						{
							string[] f = new string[2];
							f[0] = files[0].FullName;
							f[1] = files[1].FullName;
							this.ErrorEvent(e, f);
						}
						break;

					case FileActions.Write2to1:	// Writes the file to folder 1 (the file should exist)
						try
						{
							((FileInfo)files[1]).CopyTo(files[0].FullName, true);
						}
						catch (Exception e)  // This should NOT happen (and it doesn't, usually...)
						{
							string[] f = new string[2];
							f[0] = files[0].FullName;
							f[1] = files[1].FullName;
							this.ErrorEvent(e, f);
						}
						break;						
				}
			}
			#endregion

			#region MissingInFolderx
			if (result ==  ComparisonResult.MissingInFolder1 | result ==  ComparisonResult.MissingInFolder2)
			{
				FileActions action = FileActions.Ask;
				int missingIndex = 0;	// used only for the AskWhatToDo event 
										// if default action is Ask

				if (result == ComparisonResult.MissingInFolder1)
				{
					action = this.defMissing1;
					missingIndex = 1;
				}
				if (result == ComparisonResult.MissingInFolder2)
				{
					action = this.defMissing2;
					missingIndex = 2;
				}

				if (action == FileActions.Ask)
					action = this.AskWhatToDo(files, isADir, missingIndex);

				if (action == FileActions.CancelCopy)
				{
					this.CancelNow();
					return;
				}

				switch (action)
				{
					case FileActions.Ignore:
						break;

					case FileActions.Copy:					// Copy the missing file or dir
						
						if (!isADir) // It's not a dir (incredible!)
						{
							int missingFileIndex = missingIndex - 1; // missingFileIndex has to be 0-based
							int presentFileIndex = Math.Abs(missingFileIndex - 1);

							try
							{
								((FileInfo)files[presentFileIndex]).CopyTo(files[missingFileIndex].FullName, false);
							}
							catch (IOException e) // This should not happen because files[presentIndex] should not exist
							{
								string[] f = new string[2];
								f[0] = files[0].FullName;
								f[1] = files[1].FullName;
								this.ErrorEvent(e, f);
							}

						}
					
						else if (isADir)	// It's a dir (wow!)
						{
							int missingFolderIndex = missingIndex - 1;
							int presentFolderIndex = Math.Abs(missingFolderIndex - 1);

							RecursiveCopy rCopy = new RecursiveCopy(files[presentFolderIndex].FullName, files[missingFolderIndex].FullName, false);
							rCopy.ErrorEvent += this.ErrorEvent;
							rCopy.Copy();
						}
						break;

					case FileActions.Delete:		// The existing file or dir will be deleted
						if (isADir)
						{
							if (files[0].Exists)
								((DirectoryInfo)files[0]).Delete(true);
							else if (files[1].Exists)
								((DirectoryInfo)files[1]).Delete(true);
						}
						else
						{
							if (files[0].Exists)
								((FileInfo)files[0]).Delete();
							else if (files[1].Exists)
								((FileInfo)files[1]).Delete();
						}
						break;

					case FileActions.Write1to2:		// Shall write the file or dir from folder index 0 to folder index 1
						if (files[0].Exists & isADir)
						{
							RecursiveCopy rCopy = new RecursiveCopy(files[0].FullName, files[1].FullName, false);
							rCopy.ErrorEvent += this.ErrorEvent;
							rCopy.Copy();
						}
						else if (files[0].Exists & !isADir)
						{
							try
							{
								((FileInfo)files[0]).CopyTo(files[1].FullName, false);	// Copies the file
							}
							catch (Exception e)
							{
								string[] f = new string[2];
								f[0] = files[0].FullName;
								f[1] = files[1].FullName;
								this.ErrorEvent(e, f);
							}
						}
						break;

					case FileActions.Write2to1: // Shall write the file or dir from folder index 1 to folder index 0
						if (files[1].Exists & isADir)
						{
							RecursiveCopy rCopy = new RecursiveCopy(files[1].FullName, files[0].FullName, false);
							rCopy.ErrorEvent += this.ErrorEvent;
							rCopy.Copy();
						}
						else if (files[1].Exists & !isADir)
						{
							try
							{
								((FileInfo)files[1]).CopyTo(files[0].FullName, false);
							}
							catch (Exception e)
							{
								string[] f = new string[2];
								f[0] = files[0].FullName;
								f[1] = files[1].FullName;
								this.ErrorEvent(e, f);
							}
						}
						break;
				}
			}
			#endregion
		}

		/// <summary>
		/// Begins synchronisation.
		/// </summary>
		public void Sync()
		{
			if ((this.defMissing1 == FileActions.Ask | this.defMissing2 == FileActions.Ask
				| this.defSize == FileActions.Ask) & this.AskWhatToDo == null)
				return;

			if (this.initialized & this.ErrorEvent != null)
				diff.Compare();
		}
        
		/// <summary>
		/// Copies one file over another. Looks for the last modification date to know which file
		/// to overwrite.
		/// </summary>
		/// <param name="file1">the first file</param>
		/// <param name="file2">the second file</param>
		/// <param name="deleteOlder">true deletes the older file, false the newer</param>		
		private void OverWriteDate(FileInfo file1, FileInfo file2, bool deleteOlder) 
		{
			long fileModDate1 = file1.LastWriteTime.Ticks;
			long fileModDate2 = file2.LastWriteTime.Ticks;
			// Ticks bigger => file newer
			if (deleteOlder ^ (fileModDate1 < fileModDate2))
			{
				// delete file 2 ==> Copy 1 -> 2
				try
				{
					file1.CopyTo(file2.FullName, true);
				}
				catch (IOException e)
				{
					string[] f = new string[2];
					f[0] = file1.FullName;
					f[1] = file2.FullName ;
					this.ErrorEvent(e,f);
				}
			}
			else
			{
				try
				{
				// delete file 1 ==> Copy 2 -> 2
				file2.CopyTo(file1.FullName, true);
				}
				catch (IOException e)
				{
					string[] f = new string[2];
					f[0] = file1.FullName;
					f[1] = file2.FullName ;
					this.ErrorEvent(e,f);
				}
			}
		}
	}

	/// <summary>
	/// Can copy dirs recursively (That's magic)
	/// </summary>
	public class RecursiveCopy 
	{
		private string sourceFolder;
		public string SourceFolder { get { return this.sourceFolder; } }

		private string destinationFolder;
		public string DestinationFolder { get { return this.destinationFolder;	} }

		private bool overwrite;
		public bool Overwrite { get { return this.overwrite; } }

		private bool cancelAll = false;
		private bool isReady = false;

		public event ErrorDelegate ErrorEvent;

		/// <summary>
		/// Copies a directory to another. Recursively.
		/// </summary>
		/// <param name="sourceFolder">Source Dir</param>
		/// <param name="destinationFolder">Destination dir.</param>
		/// <param name="overwrite">Specifies if a existing dir shall be overwritten. Existing files will not be deleted</param>
		public RecursiveCopy(string sourceFolder, string destinationFolder, bool overwrite)
		{
			this.sourceFolder = sourceFolder;
			this.destinationFolder = destinationFolder;
			this.overwrite = true;
			if (Directory.Exists(this.destinationFolder) & !overwrite)
			{
				this.ErrorEvent(new ApplicationException("The destination directory exists and overwrite is false!"), null);
			}
			isReady = true;

		}

		/// <summary>
		/// Cancels all copying
		/// </summary>
		public void CancelAll()
		{
			this.cancelAll = true;
		}

		/// <summary>
		/// Begins the copy process.
		/// </summary>
		public void Copy()
		{
			if (!isReady)
				return;
			DirectoryInfo sourceDir = new DirectoryInfo(this.sourceFolder);
			DirectoryInfo destDir = new DirectoryInfo(this.destinationFolder);
			Directory.CreateDirectory(this.destinationFolder);
			Copy(sourceDir, destDir);
		}

		private void Copy(DirectoryInfo sourceDir, DirectoryInfo destDir)
		{
			DirectoryInfo[] subDirs = sourceDir.GetDirectories();
			FileInfo[] files = sourceDir.GetFiles();
			foreach (FileInfo file in files)
			{
				if (cancelAll)
					return;
				try
				{
					file.CopyTo(destDir.FullName + "\\" + file.Name, this.overwrite);
				}
				catch (IOException e)
				{
					string[] f= new string[1];
					f[0] = file.FullName;
					this.ErrorEvent(e, f);
				}
			}

			foreach (DirectoryInfo dir in subDirs)
			{
				if (cancelAll)
					return;
				Directory.CreateDirectory(destDir.FullName + @"\" + dir.Name);
				Copy(dir, new DirectoryInfo(destDir.FullName + @"\" + dir.Name));
			}
		}
	}
}
