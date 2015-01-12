using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
//using Microsoft.WindowsAzure;
using System.Configuration;
using System.Web.Hosting;
//using Microsoft.WindowsAzure.StorageClient;

namespace sGridServer.Code.Utilities
{
    /// <summary>
    /// This class provides a simple way to store Blobs 
    /// (large binary objects) persistently, building 
    /// on the Azure blob storage service.
    /// </summary>
    public class BlobStorage
    {
        /// <summary>
        /// Gets the name of the current blob container. 
        /// </summary>
        public string Container { get; private set; }

        /// <summary>
        /// Gets the physical container path of the current blob container. 
        /// </summary>
        public string PhysicalContainer { get; private set; }

        /// <summary>
        /// Gets the virtual container path of the current blob container. 
        /// </summary>
        public string VirtualContainer { get; private set; }

        /// <summary>
        /// Creates a new instance of this class, associating it with the given storage container. 
        /// </summary>
        /// <remarks>
        /// The container name has to be between 3 and 63 characters long, contain neither space characters nor special caracters. 
        /// The container name is automatically convertedt to lowercase for internal use, and therefore is 
        /// case invariant. 
        /// </remarks>
        /// <param name="container">The name of the container to associate with the new object.</param>
        public BlobStorage(string container)
        {
            if (container.Length < 3 || container.Length > 63)
            {
                throw new ArgumentException("Container name must be between 3 and 63 characters long");
            }
            if (container.IndexOfAny(Path.GetInvalidPathChars()) > 0)
            {
                throw new ArgumentException("Invalid container name.");
            }


            string root = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, sGridServer.Properties.Settings.Default.BlobRoot);
            string virtualRoot = "http://" + HttpContext.Current.Request.Url.Authority + "/" + sGridServer.Properties.Settings.Default.BlobRoot + "/";
            this.Container = container.ToLowerInvariant();
            this.PhysicalContainer = Path.Combine(root, this.Container);
            this.VirtualContainer = new Uri(new Uri(virtualRoot), this.Container + "/").ToString();

            if (!Directory.Exists(PhysicalContainer))
            {
                Directory.CreateDirectory(PhysicalContainer);
            }
        }

        /// <summary>
        /// Gets a blob from the container by its name. 
        /// </summary>
        /// <param name="name">The name of the blob to get. </param>
        /// <param name="stream">The stream to download the blob to.</param>
        public void GetBlob(string name, Stream stream)
        {
            string filePath = Path.Combine(PhysicalContainer, name);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file does not exist");
            }

            Stream fileStream = File.OpenRead(filePath);

            fileStream.CopyTo(stream);

            fileStream.Close();
        }

        /// <summary>
        /// Removes a blob from the container by its name. 
        /// </summary>
        /// <param name="name">The name of the blob to remove. </param>
        public void RemoveBlob(string name)
        {

            string filePath = Path.Combine(PhysicalContainer, Path.GetFileName(name));

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file does not exist");
            }

            File.Delete(filePath);
        }

        /// <summary>
        /// Stores the given blob and returns a generated unique 
        /// name which can be used to identify the blob later. 
        /// </summary>
        /// <param name="blob">The data of the blob to store, as stream.</param>
        /// <returns>The unique uri of the just stored blob. </returns>
        public string StoreBlob(Stream blob)
        {
            string name;
            string filePath;

            do{
                name = BitConverter.ToString(Guid.NewGuid().ToByteArray()).Replace("-", "");
                filePath = Path.Combine(PhysicalContainer, name);
            } while(File.Exists(filePath));

            return StoreBlob(blob, name);
        }

        /// <summary>
        /// Stores the given blob using the given name. 
        /// If another blob already exists with this name, it is overwritten. 
        /// </summary>
        /// <param name="blob">The data of the blob to store, as stream.</param>
        /// <param name="name">The name which should be used to store the given blob. </param>
        /// <returns>The unique url of the just stored blob. </returns>
        public string StoreBlob(Stream blob, string name)
        {
            string filePath = Path.Combine(PhysicalContainer, name);

            Stream fileStream = File.Open(filePath, FileMode.Create);

            blob.CopyTo(fileStream);

            fileStream.Close();

            return new Uri(new Uri(VirtualContainer), name).ToString();
        }
    }
}