namespace DiagramGenerator.ClassGraph;

public interface IGraphBuilder
{
    /// <summary>
    /// 类型过滤条件:
    ///   属于指定dll中的指定namespace中, 或者属于指定dll中的指定类型
    /// </summary>
    /// <param name="files"></param>
    /// <param name="nsList"></param>
    /// <param name="typenameList"></param>
    /// <returns></returns>
    Graph Build(IEnumerable<string> files, IEnumerable<string> nsList, IEnumerable<string> typenameList, bool inheretanceOnly);
}