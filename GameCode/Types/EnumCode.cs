namespace GameData
{
    public struct EnumCode : ICode
    {
        public string[] GetCode()
        {
            const string code = """
                                namespace enums
                                {

                                }

                                """;
            return code.Split("\n");
        }
    }
}
