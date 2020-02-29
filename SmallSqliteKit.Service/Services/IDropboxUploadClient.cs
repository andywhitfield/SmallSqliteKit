using System.IO;
using System.Threading.Tasks;

namespace SmallSqliteKit.Service.Services
{
    public interface IDropboxUploadClient
    {
        Task UploadFileAsync(FileInfo file, string uploadWithFilename);
    }
}