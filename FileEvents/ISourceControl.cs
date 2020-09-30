namespace FileEvents
{
	public interface ISourceControl
	{
		void Add(string path);
		Document Preview(string path, int bookmark);
	}
}
