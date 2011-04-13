﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.FtpClient;
using System.IO;

namespace ReleaseTests {
	class Program {
		static void RecursiveDownload(FtpDirectory dir, string local) {
			if (!Directory.Exists(local)) {
				Directory.CreateDirectory(local);
			}

			foreach (FtpFile f in dir.Files) {
				f.Download(string.Format("{0}\\{1}", local, f.Name));
			}

			foreach (FtpDirectory d in dir.Directories) {
				RecursiveDownload(d, string.Format("{0}\\{1}", local, d.Name));
			}
		}

		static void RecursiveUpload(FtpDirectory remote, DirectoryInfo local) {
			foreach (FileInfo f in local.GetFiles()) {
				FtpFile ff = new FtpFile(remote.Client, string.Format("{0}/{1}", remote.FullName, f.Name));

				if (!ff.Exists) {
					ff.Upload(f.FullName);
				}
			}

			foreach (DirectoryInfo d in local.GetDirectories()) {
				if (!remote.DirectoryExists(d.Name)) {
					remote.CreateDirectory(d.Name);
				}

				RecursiveUpload(new FtpDirectory(remote.Client, string.Format("{0}/{1}", remote.FullName, d.Name)), d);
			}
		}

		static void RecursiveDelete(FtpDirectory dir) {
			foreach (FtpFile f in dir.Files) {
				Console.WriteLine("X: {0}", f.FullName);
				f.Delete();
			}

			foreach (FtpDirectory d in dir.Directories) {
				RecursiveDelete(d);
				Console.WriteLine("X: {0}", d.FullName);
				d.Delete();
			}
		}

		static void Main(string[] args) {
			using (FtpClient cl = new FtpClient("test", "test", "localhost")) {
				cl.IgnoreInvalidSslCertificates = true;
				cl.TransferProgress += new TransferProgress(cl_TransferProgress);

				RecursiveDownload(cl.CurrentDirectory, "c:\\temp");
				RecursiveDelete(cl.CurrentDirectory);
				RecursiveUpload(cl.CurrentDirectory, new DirectoryInfo("c:\\temp"));
			}
		}

		static void cl_TransferProgress(FtpTransferInfo e) {
			Console.Write("\r{0}: {1} {2}/{3} {4}/s {5}%",
				e.TransferType == FtpTransferType.Upload ? "U" : "D",
				Path.GetFileName(e.RemoteFile), e.Transferred, e.Length,
				e.BytesPerSecond, e.Percentage);

			if (e.Complete) {
				Console.WriteLine();
			}
		}
	}
}