using System.Text.Json;

namespace ZrConfig.Core;

/// <summary>Declaration syntax model for the generated native-language subset. This is an
/// independent reader of emitted text, not the generator's symbol plan. Opaque source digests
/// remain mandatory: a declaration model alone cannot prove behavior or compiler binding.</summary>
internal static class NativeApiSurface
{
    internal static GeneratedApiSymbol[] Read(string path, string source)
    {
        string language = Path.GetExtension(path) switch { ".java"=>"java", ".go"=>"go", ".hpp"=>"cpp", ".rs"=>"rust", ".ts"=>"typescript", ".lua"=>"lua", _=>"" };
        if(language.Length==0)return [];
        var parser=new Parser(language,path,Tokens(source,language));
        return parser.Read().Distinct().OrderBy(s=>s.Scope,StringComparer.Ordinal).ThenBy(s=>s.Name,StringComparer.Ordinal).ThenBy(s=>s.Signature,StringComparer.Ordinal).ToArray();
    }
    private static List<string> Tokens(string source,string language)
    {
        var tokens=new List<string>();
        for(int i=0;i<source.Length;)
        {
            char c=source[i];
            if(char.IsWhiteSpace(c)){if(c=='\n' && language is "go" or "lua")tokens.Add("\n");i++;continue;}
            if(language=="cpp"&&c=='#') {int end=source.IndexOf('\n',i);i=end<0?source.Length:end;continue;}
            bool luaComment=language=="lua"&&source.AsSpan(i).StartsWith("--"),comment=language!="lua"&&source.AsSpan(i).StartsWith("//");
            if(luaComment||comment){int end=source.IndexOf('\n',i);i=end<0?source.Length:end;continue;}
            if(language!="lua"&&source.AsSpan(i).StartsWith("/*"))
            {
                int depth=1;i+=2;
                while(i<source.Length&&depth>0){if(language=="rust"&&source.AsSpan(i).StartsWith("/*")){depth++;i+=2;}else if(source.AsSpan(i).StartsWith("*/")){depth--;i+=2;}else i++;}
                if(depth!=0)throw new InvalidDataException("Unterminated declaration comment.");continue;
            }
            if(language=="lua"&&c=='[')
            {
                int openingEnd=i+1;while(openingEnd<source.Length&&source[openingEnd]=='=')openingEnd++;
                if(openingEnd<source.Length&&source[openingEnd]=='[')
                {
                    string close="]"+new string('=',openingEnd-i-1)+"]";int contentStart=openingEnd+1;
                    int end=source.IndexOf(close,contentStart,StringComparison.Ordinal);if(end<0)throw new InvalidDataException("Unterminated Lua literal.");
                    tokens.Add(JsonSerializer.Serialize(source.Substring(contentStart,end-contentStart)));i=end+close.Length;continue;
                }
            }
            if(language=="typescript"&&c=='/'&&(tokens.Count==0||tokens[^1] is "(" or "=" or ":" or "," or "!" or "?" or ">" or "return" or "["))
            {
                int start=i++;bool characterClass=false,complete=false;
                while(i<source.Length){char part=source[i++];if(part=='\\'){i++;continue;}if(part=='[')characterClass=true;if(part==']')characterClass=false;if(part=='/'&&!characterClass){complete=true;break;}}
                if(!complete)throw new InvalidDataException("Unterminated regular expression.");while(i<source.Length&&char.IsLetter(source[i]))i++;tokens.Add(source[start..i]);continue;
            }
            if(c is '"' or '\'' or '`')
            {
                if(language=="rust"&&c=='\''&&i+1<source.Length&&(char.IsLetter(source[i+1])||source[i+1]=='_')&&(i+2>=source.Length||source[i+2]!='\'')){tokens.Add("'");i++;continue;}
                int start=i++;bool complete=false;
                while(i<source.Length){if(source[i]=='\\'&&(c!='`'||language!="go")){i+=2;continue;}if(source[i++]==c){complete=true;break;}}
                if(!complete)throw new InvalidDataException("Unterminated declaration literal.");tokens.Add(source[start..i]);continue;
            }
            if(char.IsLetterOrDigit(c)||c=='_'||c=='$')
            {int start=i++;while(i<source.Length&&(char.IsLetterOrDigit(source[i])||source[i] is '_' or '$'))i++;tokens.Add(source[start..i]);continue;}
            tokens.Add(c.ToString());i++;
        }
        return tokens;
    }
    private sealed class Parser(string language,string path,List<string> tokens)
    {
        private readonly List<GeneratedApiSymbol> _symbols=[];
        private readonly Dictionary<int,int> _ends=[];
        private readonly HashSet<string> _publicTypes=new(StringComparer.Ordinal);
        private string Root=>language+":"+path;
        internal IEnumerable<GeneratedApiSymbol> Read()
        {
            if(language=="lua"){Lua();return _symbols;}
            var stack=new Stack<int>();
            for(int i=0;i<tokens.Count;i++)
            {
                if(tokens[i] is "(" or "[" or "{")stack.Push(i);
                else if(tokens[i] is ")" or "]" or "}")
                {
                    if(stack.Count==0)throw new InvalidDataException("Unbalanced declaration syntax: "+path);
                    int open=stack.Pop();if(tokens[open]+tokens[i] is not ("()" or "[]" or "{}"))throw new InvalidDataException("Mismatched declaration syntax: "+path);
                    _ends[open]=i;
                }
            }
            if(stack.Count!=0)throw new InvalidDataException("Unclosed declaration syntax: "+path);
            Walk(0,tokens.Count,Root,true,"module");return _symbols;
        }
        private void Add(string scope,string name,IEnumerable<string> declaration)
        {string signature=string.Join(" ",declaration.Where(t=>t!="\n"));if(signature.Length!=0)_symbols.Add(new(scope,name,signature));}
        private static bool Id(string token)=>token.Length>0&&(char.IsLetter(token[0])||token[0] is '_' or '$');
        private bool Public(List<string> header,bool visible,string kind,bool access)
        {
            if(!visible)return false;
            return language switch
            {
                "java"=>header.Contains("public")||header.Contains("protected")||kind=="interface",
                "cpp"=>access,
                "rust"=>header.Contains("pub")&&!header.Contains("super")&&!header.Contains("crate")||kind=="trait",
                "typescript"=>kind=="module"?header.Contains("export"):!header.Contains("private")&&!header.Contains("#"),
                "go"=>true,
                _=>false
            };
        }
        private void Walk(int begin,int end,string scope,bool visible,string kind)
        {
            bool access=kind!="class"||language!="cpp";
            for(int i=begin;i<end;)
            {
                while(i<end&&tokens[i] is ";" or "\n" or ",")i++;
                if(i>=end)break;
                if(language=="cpp"&&tokens[i] is "public" or "protected" or "private"&&i+1<end&&tokens[i+1]==":")
                {access=tokens[i]!="private";i+=2;continue;}
                int start=i,delimiter=end;
                while(i<end)
                {
                    if(tokens[i] is "(" or "["){i=_ends[i]+1;continue;}
                    if(tokens[i] is ";" or "{" || language=="go"&&tokens[i]=="\n" || language=="rust"&&kind=="struct"&&tokens[i]==","){delimiter=i;break;}
                    i++;
                }
                var header=tokens.GetRange(start,delimiter-start);bool block=delimiter<end&&tokens[delimiter]=="{";
                int next=block?_ends[delimiter]+1:Math.Min(end,delimiter+1);i=next;
                if(header.Count==0)continue;
                if(language=="go"&&header[0] is "const" or "var")
                {
                    if(header.Count>1&&header[1]=="(")
                    {bool exportedConstant=false;for(int n=2;n<header.Count;n++)if((n==2||header[n-1]=="\n")&&Id(header[n])&&char.IsUpper(header[n][0]))exportedConstant=true;if(visible&&exportedConstant)Add(scope,"$"+header[0],header);}
                    else if(header.Count>1&&char.IsUpper(header[1][0])&&visible)Add(scope,header[1],header);
                    continue;
                }
                int type=header.FindIndex(t=>t is "class" or "struct" or "interface" or "enum" or "record" or "namespace" or "mod" or "trait" or "impl");
                if(language=="go"&&header[0]=="type")type=0;
                if(type>=0)
                {
                    string declarationKind=header[type],name;
                    if(declarationKind=="impl")
                    {
                        int target=header.IndexOf("for");if(target>=0)name=header.Skip(target+1).FirstOrDefault(Id)??"impl";
                        else{int n=type+1,depth=0;if(n<header.Count&&header[n]=="<"){do{if(header[n]=="<")depth++;if(header[n]==">")depth--;n++;}while(n<header.Count&&depth>0);}name=header.Skip(n).FirstOrDefault(Id)??"impl";}
                        bool visibleImplementation=_publicTypes.Contains(name);if(visibleImplementation&&header.Contains("for"))Add(scope,name+"::trait",header);
                        if(block)Walk(delimiter+1,next-1,scope+"::"+name,visibleImplementation,header.Contains("for")?"trait":"impl");continue;
                    }
                    name=header.Skip(type+1).FirstOrDefault(Id)??"$anonymous";
                    if(language=="go"){name=header[1];declarationKind=header.Contains("interface")?"interface":header.Contains("struct")?"struct":"alias";}
                    bool space=declarationKind is "namespace" or "mod";
                    bool exported=space?visible&&(language!="rust"||header.Contains("pub"))&&name!="detail":Public(header,visible,kind,access);
                    if(language=="go")exported=visible&&char.IsUpper(name[0]);
                    if(exported){_publicTypes.Add(name);Add(scope,name,header);}
                    if(block)
                    {
                        if(declarationKind=="enum"&&exported)Add(scope+"::"+name,"$variants",tokens.GetRange(delimiter+1,next-delimiter-2));
                        else Walk(delimiter+1,next-1,scope+"::"+name,exported,declarationKind);
                    }
                    continue;
                }
                if(header[0] is "package" or "import" or "use" || header.Count>1&&header[0]=="pub"&&header[1]=="use")
                {if(kind=="module")Add(scope,"$imports",header);continue;}
                bool isPublic=Public(header,visible,kind,access);string member=Member(header);
                if(language=="go")isPublic=visible&&(member=="$constants"||member.Length>0&&char.IsUpper(member[0]));
                string memberScope=scope;
                if(language=="go"&&header.Count>3&&header[0]=="func"&&header[1]=="(")
                {string? owner=header.Skip(3).TakeWhile(t=>t!=")").FirstOrDefault(Id);if(owner!=null)memberScope+="::"+owner;}
                if(isPublic)Add(memberScope,member,header);
            }
        }
        private string Member(List<string> header)
        {
            if(language=="go")
            {
                if(header[0]=="const")return "$constants";
                if(header[0]=="func")
                {int at=1;if(at<header.Count&&header[at]=="("){int depth=1;for(at++;at<header.Count&&depth>0;at++){if(header[at]=="(")depth++;if(header[at]==")")depth--;}}return at<header.Count?header[at]:"$function";}
                return header.FirstOrDefault(Id)??"$member";
            }
            int function=header.IndexOf("fn");if(function<0)function=header.IndexOf("function");
            if(function>=0)return header.Skip(function+1).FirstOrDefault(Id)??"$function";
            int named=header.FindIndex(t=>t is "const" or "let" or "var" or "type" or "using");
            if(language is "typescript" or "rust"&&named>=0)return header.Skip(named+1).FirstOrDefault(Id)??"$member";
            if(language=="cpp"&&header[0]=="using")return header.Skip(1).FirstOrDefault(Id)??"$alias";
            int args=-1,angle=0;for(int n=0;n<header.Count;n++){if(header[n]=="<")angle++;if(header[n]==">")angle--;if(header[n]=="("&&angle==0){args=n;break;}}if(args>=0)
            {
                int at=args-1;
                if(at>=0&&header[at]==">"){int depth=1;for(at--;at>=0&&depth>0;at--){if(header[at]==">")depth++;if(header[at]=="<")depth--;}}
                while(at>=0&&!Id(header[at])&&header[at]!="~")at--;
                return at>=0?header[at]:"$call";
            }
            int assign=header.IndexOf("=");if(assign<0)assign=header.Count;
            if(language is "typescript" or "rust"){int colon=header.IndexOf(":");if(colon>=0)assign=colon;}
            return header.Take(assign).LastOrDefault(Id)??"$member";
        }
        private void Lua()
        {
            for(int i=0;i<tokens.Count;i++)
            {
                if(tokens[i]=="function"&&i+4<tokens.Count&&Id(tokens[i+1])&&tokens[i+2] is "." or ":"&&Id(tokens[i+3])&&tokens[i+4]=="(")
                {
                    int end=i+5;while(end<tokens.Count&&tokens[end]!=")")end++;
                    Add(Root+"::"+tokens[i+1],tokens[i+3],tokens.GetRange(i,Math.Min(end+1,tokens.Count)-i));
                }
                else if(tokens[i] is "R" or "M"&&i+3<tokens.Count&&tokens[i+1]=="."&&Id(tokens[i+2])&&tokens[i+3]=="=")
                {
                    int end=i+4;while(end<tokens.Count&&tokens[end] is not ("\n" or ";" or "function"))end++;
                    Add(Root+"::"+tokens[i],tokens[i+2],tokens.GetRange(i,Math.Min(end,tokens.Count)-i));
                }
            }
        }
    }
}
