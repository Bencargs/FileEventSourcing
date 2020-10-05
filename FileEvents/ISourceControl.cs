using System.Threading.Tasks;

namespace FileEvents
{
	public interface ISourceControl
	{
		Task Add(string path);
		Task<Document> Preview(string path, int bookmark);
	}
}
