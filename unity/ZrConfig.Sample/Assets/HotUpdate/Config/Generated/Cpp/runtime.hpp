// zrconfig-api-contract:1
#pragma once
#include <atomic>
#include <cctype>
#include <deque>
#include <map>
#include <chrono>
#include <condition_variable>
#include <mutex>
// 公共运行库：所有解析和容器构建均发生在加载期间，业务查询只使用强类型对象和现成索引。
#include <algorithm>
#include <array>
#include <atomic>
#include <cmath>
#include <cstdint>
#include <cstring>
#include <filesystem>
#include <fstream>
#include <functional>
#include <iomanip>
#include <limits>
#include <locale>
#include <memory>
#include <mutex>
#include <optional>
#include <sstream>
#include <stdexcept>
#include <string>
#include <type_traits>
#include <unordered_map>
#include <unordered_set>
#include <utility>
#include <vector>

namespace zrconfig {
namespace detail { class IndexedRows; class SourceBundle; class InvalidData : public std::runtime_error { public: explicit InvalidData(const std::string& message):std::runtime_error(message){} }; }
[[noreturn]] inline void invalid(const std::string &message) { throw detail::InvalidData("配置数据无效：" + message); }
// Server API family 1. Legacy exceptions and borrowed row APIs remain unchanged.
enum class ErrorCode {
    NotInitialized, ManagerClosed, ReloadBusy, StaleCandidate, AlreadyCommitted,
    ForeignCandidate, Cancelled, DeadlineExceeded, InvalidPlan, UnsupportedCapability,
    SourceUnavailable, InvalidManifest, TableSetMismatch, IntegrityMismatch, SchemaMismatch,
    InvalidValue, MissingReference, KeyNotFound, TableNotSelected, TableNotLoaded, RowNotLoaded, LimitExceeded,
    VersionIdentityConflict, CatalogMismatch, InternalError, InvalidEncoding, ReadFailed, BudgetExceeded, UnsupportedFormat
};
struct ErrorContext { std::string version_id,table,row_key,field_path,relative_path; };
class ConfigError : public std::runtime_error {
public:
    const ErrorCode code;
    const std::string stage;
    const std::uint64_t operation_id;
    const ErrorContext context;
    const std::exception_ptr cause;
    ConfigError(ErrorCode code, std::string message, std::string stage = "", std::uint64_t operation = 0,ErrorContext context={},std::exception_ptr cause={})
        : std::runtime_error(std::move(message)), code(code), stage(std::move(stage)), operation_id(operation),context(std::move(context)),cause(std::move(cause)) {}
    ConfigError locate(std::string phase,ErrorContext details)const {
        if(!context.version_id.empty())details.version_id=context.version_id;
        if(!context.table.empty())details.table=context.table;
        if(!context.row_key.empty())details.row_key=context.row_key;
        if(!context.field_path.empty())details.field_path=context.field_path;
        if(!context.relative_path.empty())details.relative_path=context.relative_path;
        return ConfigError(code,what(),stage.empty()?std::move(phase):stage,operation_id,std::move(details),std::current_exception());
    }
};
class Cancellation {
    struct State{std::atomic<bool> flag{false};std::shared_ptr<const State> parent;std::optional<std::chrono::steady_clock::time_point> deadline;};
    std::shared_ptr<State> state_=std::make_shared<State>();
public:
    void cancel()const noexcept{state_->flag.store(true,std::memory_order_release);}
    bool cancelled()const noexcept{for(const State* state=state_.get();state;state=state->parent.get())if(state->flag.load(std::memory_order_acquire))return true;return false;}
    void check()const{if(cancelled())throw ConfigError(ErrorCode::Cancelled,"Operation cancelled");auto now=std::chrono::steady_clock::now();for(const State* state=state_.get();state;state=state->parent.get())if(state->deadline&&now>=*state->deadline)throw ConfigError(ErrorCode::DeadlineExceeded,"Operation deadline exceeded");}
    Cancellation child(std::chrono::steady_clock::time_point deadline)const{Cancellation token;token.state_->parent=state_;token.state_->deadline=deadline;return token;}
};
class ResourceLimits {
public:
    class Builder;
private:
    std::uint64_t manifest_=16ull<<20,table_=512ull<<20,bundle_=1ull<<30,estimated_=2ull<<30,index_estimated_=2ull<<30;
    std::size_t tables_=100000,rows_=5000000,decoded_=5000000,elements_=1000000,depth_=64,string_=16u<<20,reads_=1,decodes_=1,pending_=4096;
public:
    static Builder builder();
    std::uint64_t max_manifest_bytes()const noexcept{return manifest_;}
    std::uint64_t max_table_bytes()const noexcept{return table_;}
    std::uint64_t max_bundle_bytes()const noexcept{return bundle_;}
    std::uint64_t max_estimated_index_bytes()const noexcept{return index_estimated_;}
    std::uint64_t max_estimated_decoded_bytes()const noexcept{return estimated_;}
    std::size_t max_tables()const noexcept{return tables_;}
    std::size_t max_rows()const noexcept{return rows_;}
    std::size_t max_decoded_rows()const noexcept{return decoded_;}
    std::size_t max_container_elements()const noexcept{return elements_;}
    std::size_t max_depth()const noexcept{return depth_;}
    std::size_t max_string_bytes()const noexcept{return string_;}
    std::size_t max_concurrent_reads()const noexcept{return reads_;}
    std::size_t max_concurrent_decodes()const noexcept{return decodes_;}
    std::size_t max_pending_requests()const noexcept{return pending_;}
};
class ResourceLimits::Builder {
    ResourceLimits value_;
public:
    Builder& max_manifest_bytes(std::uint64_t v){value_.manifest_=v;return *this;}
    Builder& max_table_bytes(std::uint64_t v){value_.table_=v;return *this;}
    Builder& max_bundle_bytes(std::uint64_t v){value_.bundle_=v;return *this;}
    Builder& max_estimated_index_bytes(std::uint64_t v){value_.index_estimated_=v;return *this;}
    Builder& max_estimated_decoded_bytes(std::uint64_t v){value_.estimated_=v;return *this;}
    Builder& max_tables(std::size_t v){value_.tables_=v;return *this;}
    Builder& max_rows(std::size_t v){value_.rows_=v;return *this;}
    Builder& max_decoded_rows(std::size_t v){value_.decoded_=v;return *this;}
    Builder& max_container_elements(std::size_t v){value_.elements_=v;return *this;}
    Builder& max_depth(std::size_t v){value_.depth_=v;return *this;}
    Builder& max_string_bytes(std::size_t v){value_.string_=v;return *this;}
    Builder& max_concurrent_reads(std::size_t v){value_.reads_=v;return *this;}
    Builder& max_concurrent_decodes(std::size_t v){value_.decodes_=v;return *this;}
    Builder& max_pending_requests(std::size_t v){value_.pending_=v;return *this;}
    ResourceLimits build()const{
        if(!value_.index_estimated_||!value_.manifest_||!value_.table_||!value_.bundle_||!value_.estimated_||!value_.tables_||!value_.rows_||!value_.decoded_||!value_.elements_||!value_.depth_||!value_.string_||!value_.reads_||!value_.decodes_||!value_.pending_)throw ConfigError(ErrorCode::InvalidPlan,"Resource limits must be positive");
        if(value_.rows_>5000000||value_.decoded_>5000000||value_.elements_>1000000||value_.depth_>64||value_.string_>(16u<<20))throw ConfigError(ErrorCode::UnsupportedCapability,"Limits exceed wire capabilities");
        return value_;
    }
};
inline ResourceLimits::Builder ResourceLimits::builder(){return Builder{};}
namespace detail {
inline thread_local const ResourceLimits* decode_policy=nullptr;
class DecodeScope {
    const ResourceLimits* previous_;
public:
    explicit DecodeScope(const ResourceLimits& limits):previous_(decode_policy){decode_policy=&limits;}
    ~DecodeScope(){decode_policy=previous_;}
    DecodeScope(const DecodeScope&)=delete;DecodeScope& operator=(const DecodeScope&)=delete;
};
inline std::size_t decode_elements(){return decode_policy?decode_policy->max_container_elements():8000000;}
inline std::size_t decode_depth(){return decode_policy?decode_policy->max_depth():64;}
inline std::size_t decode_string_bytes(){return decode_policy?decode_policy->max_string_bytes():16u<<20;}
inline void decode_limit(const std::string& message){if(decode_policy)throw ConfigError(ErrorCode::LimitExceeded,message);invalid(message);}
inline void decode_encoding(){if(decode_policy)throw ConfigError(ErrorCode::InvalidEncoding,"Invalid UTF-8");invalid("UTF-8 无效");}
[[noreturn]]inline void field_failure(const std::string& prefix){
    try{throw;}catch(const ConfigError& e){auto context=e.context;context.field_path=prefix+(context.field_path.empty()||context.field_path[0]=='['?"":".")+context.field_path;throw ConfigError(e.code,e.what(),e.stage,e.operation_id,std::move(context),std::current_exception());}catch(const InvalidData& e){if(!decode_policy)throw;throw ConfigError(ErrorCode::InvalidValue,e.what(),"decode",0,{"","","",prefix,""},std::current_exception());}
}

}
// Sources transfer ownership of returned bytes. The runtime never keeps the source alive.
class ConfigSource {
public:
    virtual ~ConfigSource()=default;
    virtual std::vector<std::uint8_t> read(const std::string& relative_path,std::uint64_t maximum_bytes,const Cancellation& cancellation)=0;
};
inline void validate_source_path(const std::string& name){
    if(name.empty()||name.find_first_of("\\:")!=std::string::npos||name.find('\0')!=std::string::npos)throw ConfigError(ErrorCode::InvalidPlan,"Invalid relative source path");
    std::size_t start=0;while(true){auto end=name.find('/',start);auto part=name.substr(start,end==std::string::npos?end:end-start);if(part.empty()||part[0]=='.')throw ConfigError(ErrorCode::InvalidPlan,"Invalid relative source path");if(end==std::string::npos)break;start=end+1;}
}
class MemoryConfigSource final:public ConfigSource {
    std::unordered_map<std::string,std::vector<std::uint8_t>> files_;
public:
    explicit MemoryConfigSource(std::unordered_map<std::string,std::vector<std::uint8_t>> files):files_(std::move(files)){for(const auto& file:files_)validate_source_path(file.first);}
    std::vector<std::uint8_t> read(const std::string& name,std::uint64_t maximum,const Cancellation& cancellation)override{
        cancellation.check();validate_source_path(name);if(!maximum)throw ConfigError(ErrorCode::InvalidPlan,"Invalid byte limit");auto found=files_.find(name);if(found==files_.end())throw ConfigError(ErrorCode::SourceUnavailable,"Source file missing");if(found->second.size()>maximum)throw ConfigError(ErrorCode::LimitExceeded,"Source file exceeds limit");return found->second;
    }
};
class FileConfigSource final:public ConfigSource {
    std::filesystem::path root_;
    std::filesystem::path checked_path(const std::string& name)const{
        auto root_status=std::filesystem::symlink_status(root_);if(!std::filesystem::is_directory(root_status)||std::filesystem::is_symlink(root_status))throw ConfigError(ErrorCode::ReadFailed,"Source root must be a real directory");
        auto path=std::filesystem::canonical(root_);for(const auto& part:std::filesystem::path(name)){path/=part;if(std::filesystem::is_symlink(std::filesystem::symlink_status(path)))throw ConfigError(ErrorCode::ReadFailed,"Symbolic links are not permitted");}return path;
    }
public:
    explicit FileConfigSource(std::filesystem::path root):root_(std::move(root)){}
    std::string source_namespace()const{return std::filesystem::absolute(root_).lexically_normal().generic_string();}
    std::vector<std::uint8_t> read(const std::string& name,std::uint64_t maximum,const Cancellation& cancellation)override{
        cancellation.check();validate_source_path(name);if(!maximum)throw ConfigError(ErrorCode::InvalidPlan,"Invalid byte limit");
        try{
            auto path=checked_path(name);if(!std::filesystem::is_regular_file(path))throw ConfigError(ErrorCode::ReadFailed,"Source is not a regular file");auto size=std::filesystem::file_size(path);auto stamp=std::filesystem::last_write_time(path);
            if(size>maximum||size>(std::numeric_limits<std::size_t>::max)())throw ConfigError(ErrorCode::LimitExceeded,"Source file exceeds limit");
            std::ifstream input(path,std::ios::binary);if(!input)throw ConfigError(ErrorCode::ReadFailed,"Cannot open source file");std::vector<std::uint8_t> bytes(static_cast<std::size_t>(size));
            for(std::size_t position=0;position<bytes.size();){cancellation.check();auto count=std::min<std::size_t>(1u<<20,bytes.size()-position);if(!input.read(reinterpret_cast<char*>(bytes.data()+position),static_cast<std::streamsize>(count)))throw ConfigError(ErrorCode::ReadFailed,"Short source read");position+=count;}
            cancellation.check();char extra;if(input.get(extra)||!input.eof())throw ConfigError(ErrorCode::ReadFailed,"Source grew or read failed");
            if(checked_path(name)!=path||std::filesystem::file_size(path)!=size||std::filesystem::last_write_time(path)!=stamp)throw ConfigError(ErrorCode::ReadFailed,"Source changed during reading");
            return bytes;
        }catch(const std::filesystem::filesystem_error& error){throw ConfigError(error.code()==std::errc::no_such_file_or_directory?ErrorCode::SourceUnavailable:ErrorCode::ReadFailed,error.what());}
    }
};
struct SnapshotDiagnostics {
    std::uint64_t read_bytes=0,retained_raw_bytes=0,estimated_decoded_bytes=0,merged_requests=0,faulted_rows=0,faulted_indexes=0,estimated_index_bytes=0;
    std::size_t declared_rows=0,decoded_rows=0,active_decodes=0,pending_jobs=0,pending_waiters=0;
    const char* health()const noexcept{return (faulted_rows||faulted_indexes)?"Degraded":"Healthy";}
};
namespace detail {
class LoadBudget {
    ResourceLimits limits_;mutable std::mutex gate_;SnapshotDiagnostics state_;
    [[noreturn]] static void exhausted(){throw ConfigError(ErrorCode::LimitExceeded,"Load budget exhausted");}
public:
    explicit LoadBudget(ResourceLimits limits):limits_(limits){}
    const ResourceLimits& limits()const noexcept{return limits_;}
    SnapshotDiagnostics diagnostics()const{std::lock_guard<std::mutex> lock(gate_);return state_;}
    void read(std::size_t bytes){std::lock_guard<std::mutex> lock(gate_);if(bytes>limits_.max_bundle_bytes()-state_.read_bytes)exhausted();state_.read_bytes+=bytes;}
    void rows(std::size_t rows){std::lock_guard<std::mutex> lock(gate_);if(rows>limits_.max_rows()-state_.declared_rows)exhausted();state_.declared_rows+=rows;}
    void raw(std::size_t bytes){std::lock_guard<std::mutex> lock(gate_);state_.retained_raw_bytes+=bytes;}
    void fault(){std::lock_guard<std::mutex> lock(gate_);++state_.faulted_rows;}
    void index_fault(){std::lock_guard<std::mutex> lock(gate_);++state_.faulted_indexes;}
    std::uint64_t read_limit()const{std::lock_guard<std::mutex> lock(gate_);auto remaining=(std::min)(limits_.max_table_bytes(),limits_.max_bundle_bytes()-state_.read_bytes);if(!remaining)exhausted();return remaining;}
    class Index {
        LoadBudget& budget_;std::uint64_t bytes_;bool committed_=false;
    public:
        Index(LoadBudget& budget,std::uint64_t bytes):budget_(budget),bytes_(bytes){std::lock_guard<std::mutex> lock(budget_.gate_);if(bytes>budget_.limits_.max_estimated_index_bytes()-budget_.state_.estimated_index_bytes)throw ConfigError(ErrorCode::BudgetExceeded,"Derived index budget exhausted","group");budget_.state_.estimated_index_bytes+=bytes;}
        Index(const Index&)=delete;Index& operator=(const Index&)=delete;
        void commit(std::uint64_t retained){std::lock_guard<std::mutex> lock(budget_.gate_);if(retained>bytes_&&retained-bytes_>budget_.limits_.max_estimated_index_bytes()-budget_.state_.estimated_index_bytes)throw ConfigError(ErrorCode::BudgetExceeded,"Derived index budget exhausted","group");budget_.state_.estimated_index_bytes=budget_.state_.estimated_index_bytes-bytes_+retained;bytes_=retained;committed_=true;}
        ~Index(){if(!committed_){std::lock_guard<std::mutex> lock(budget_.gate_);budget_.state_.estimated_index_bytes-=bytes_;}}
    };
    class Request {
        LoadBudget* budget_;bool job_;
    public:
        Request(LoadBudget& budget,bool job):budget_(&budget),job_(job){std::lock_guard<std::mutex> lock(budget.gate_);auto& count=job?budget.state_.pending_jobs:budget.state_.pending_waiters;if(count>=budget.limits_.max_pending_requests())exhausted();++count;if(!job)++budget.state_.merged_requests;}
        Request(const Request&)=delete;Request& operator=(const Request&)=delete;
        ~Request(){std::lock_guard<std::mutex> lock(budget_->gate_);--(job_?budget_->state_.pending_jobs:budget_->state_.pending_waiters);}
    };
    class Decode {
        LoadBudget* budget_;std::uint64_t bytes_;bool committed_=false,transient_=false;
    public:
        Decode(LoadBudget& budget,std::uint64_t bytes,bool transient=false):budget_(&budget),bytes_(bytes),transient_(transient){std::lock_guard<std::mutex> lock(budget.gate_);auto& state=budget.state_;if(state.active_decodes>=budget.limits_.max_concurrent_decodes()||(!transient&&state.decoded_rows>=budget.limits_.max_decoded_rows())||bytes>budget.limits_.max_estimated_decoded_bytes()-state.estimated_decoded_bytes)exhausted();++state.active_decodes;if(!transient)++state.decoded_rows;state.estimated_decoded_bytes+=bytes;}
        Decode(const Decode&)=delete;Decode& operator=(const Decode&)=delete;
        void commit()noexcept{committed_=true;}
        ~Decode(){std::lock_guard<std::mutex> lock(budget_->gate_);--budget_->state_.active_decodes;if(!committed_){if(!transient_)--budget_->state_.decoded_rows;budget_->state_.estimated_decoded_bytes-=bytes_;}}
    };
};
}

constexpr std::size_t max_rows = 5000000, max_elements = 1000000;
struct Json {
    enum class Kind { Null, Bool, Number, String, Array, Object };
    Kind kind = Kind::Null;
    std::string text;
    std::vector<Json> items;
    std::unordered_map<std::string, Json> members;
    Json() = default;
    Json(Kind k, std::string t = "") : kind(k), text(std::move(t)) {}
    const Json &field(const std::string &name) const {
        if (kind != Kind::Object)
            invalid("需要对象");
        auto i = members.find(name);
        if (i == members.end())
            invalid("缺少字段：" + name);
        return i->second;
    }
    const std::vector<Json> &array() const {
        if (kind != Kind::Array)
            invalid("需要数组");
        return items;
    }
    const std::string &string() const {
        if (kind != Kind::String)
            invalid("需要字符串");
        return text;
    }
    const std::string &number() const {
        if (kind != Kind::Number)
            invalid("需要数值");
        return text;
    }
    bool boolean() const {
        if (kind != Kind::Bool)
            invalid("需要 bool");
        return text == "true";
    }
};
inline bool valid_utf8(const std::string &text) {
    const auto *s = reinterpret_cast<const unsigned char *>(text.data());
    std::size_t i = 0;
    while (i < text.size()) {
        unsigned c = s[i++];
        if (c < 128)
            continue;
        int n;
        std::uint32_t code, min;
        if (c >= 0xc2 && c <= 0xdf) {
            n = 1;
            code = c & 31;
            min = 0x80;
        } else if (c >= 0xe0 && c <= 0xef) {
            n = 2;
            code = c & 15;
            min = 0x800;
        } else if (c >= 0xf0 && c <= 0xf4) {
            n = 3;
            code = c & 7;
            min = 0x10000;
        } else
            return false;
        if (i + n > text.size())
            return false;
        for (int j = 0; j < n; j++) {
            c = s[i++];
            if ((c & 0xc0) != 0x80)
                return false;
            code = (code << 6) | (c & 63);
        }
        if (code < min || code > 0x10ffff || (code >= 0xd800 && code <= 0xdfff))
            return false;
    }
    return true;
}
namespace detail {
struct JsonAllocationBounds {
    std::size_t array, members=4, string_bytes, depth, rows;
    std::string root_array="rows";
    explicit JsonAllocationBounds(const ResourceLimits& limits):array(64),members(32),string_bytes(4096),depth(8),rows(limits.max_tables()),root_array("tables"){}
    JsonAllocationBounds(const Json& schema,const ResourceLimits& limits)
        :array((std::max)(std::size_t{64},limits.max_container_elements())),string_bytes((std::max)(std::size_t{256},limits.max_string_bytes())),depth((std::min)(std::size_t{72},limits.max_depth()*2+8)),rows(limits.max_rows()) {
        auto name=[&](const std::string& value){string_bytes=(std::max)(string_bytes,value.size());};
        auto fields=[&](const Json& values){members=(std::max)(members,values.array().size()+1);for(const auto& field:values.array())name(field.field("name").string());};
        name(schema.field("name").string());fields(schema.field("fields"));
        for(const auto& type:schema.field("types").array()){name(type.field("id").string());auto found=type.members.find("fields");if(found!=type.members.end())fields(found->second);}
    }
    [[noreturn]] static void exceeded(){throw ConfigError(ErrorCode::LimitExceeded,"JSON allocation limit exceeded","parse");}
};
}
class JsonParser {
    const std::string &text_;
    std::size_t pos_ = 0;
    std::size_t remaining_ = 32000000;
    const detail::JsonAllocationBounds* bounds_=nullptr;
    char take() {
        if (pos_ == text_.size())
            invalid("JSON 意外结束");
        return text_[pos_++];
    }
    void expect(char c) {
        if (take() != c)
            invalid("JSON 格式错误");
    }
    void space() {
        while (pos_ < text_.size() &&
               (text_[pos_] == ' ' || text_[pos_] == '\r' || text_[pos_] == '\n' || text_[pos_] == '\t'))
            pos_++;
    }
    bool consume(char c) {
        space();
        if (pos_ < text_.size() && text_[pos_] == c) {
            pos_++;
            return true;
        }
        return false;
    }
    void digits() {
        while (pos_ < text_.size() && text_[pos_] >= '0' && text_[pos_] <= '9')
            pos_++;
    }
    void required_digits() {
        auto start = pos_;
        digits();
        if (start == pos_)
            invalid("JSON 数字缺失");
    }
    std::uint32_t hex4() {
        std::uint32_t n = 0;
        for (int i = 0; i < 4; i++) {
            char c = take();
            int h = c >= '0' && c <= '9'   ? c - '0'
                    : c >= 'a' && c <= 'f' ? c - 'a' + 10
                    : c >= 'A' && c <= 'F' ? c - 'A' + 10
                                           : -1;
            if (h < 0)
                invalid("Unicode 转义无效");
            n = n * 16 + h;
        }
        return n;
    }
    static void append_utf8(std::string &s, std::uint32_t n) {
        if (n < 128)
            s.push_back(static_cast<char>(n));
        else if (n < 0x800) {
            s.push_back(static_cast<char>(0xc0 | (n >> 6)));
            s.push_back(static_cast<char>(0x80 | (n & 63)));
        } else if (n < 0x10000) {
            s.push_back(static_cast<char>(0xe0 | (n >> 12)));
            s.push_back(static_cast<char>(0x80 | ((n >> 6) & 63)));
            s.push_back(static_cast<char>(0x80 | (n & 63)));
        } else {
            s.push_back(static_cast<char>(0xf0 | (n >> 18)));
            s.push_back(static_cast<char>(0x80 | ((n >> 12) & 63)));
            s.push_back(static_cast<char>(0x80 | ((n >> 6) & 63)));
            s.push_back(static_cast<char>(0x80 | (n & 63)));
        }
    }
    std::string quoted() {
        expect('"');
        std::string s;
        while (true) {
            char c = take();
            if (c == '"')
                break;
            if (static_cast<unsigned char>(c) < 32)
                invalid("JSON 控制字符无效");
            if (c == '\\') {
                c = take();
                switch (c) {
                case '"':
                case '\\':
                case '/':
                    s.push_back(c);
                    break;
                case 'b':
                    s.push_back('\b');
                    break;
                case 'f':
                    s.push_back('\f');
                    break;
                case 'n':
                    s.push_back('\n');
                    break;
                case 'r':
                    s.push_back('\r');
                    break;
                case 't':
                    s.push_back('\t');
                    break;
                case 'u': {
                    auto n = hex4();
                    if (n >= 0xd800 && n <= 0xdbff) {
                        expect('\\');
                        expect('u');
                        auto lo = hex4();
                        if (lo < 0xdc00 || lo > 0xdfff)
                            invalid("Unicode 代理项无效");
                        n = 0x10000 + ((n - 0xd800) << 10) + lo - 0xdc00;
                    } else if (n >= 0xdc00 && n <= 0xdfff)
                        invalid("Unicode 代理项无效");
                    append_utf8(s, n);
                    break;
                }
                default:
                    invalid("JSON 转义无效");
                }
            } else
                s.push_back(c);
            if(bounds_&&s.size()>bounds_->string_bytes)detail::JsonAllocationBounds::exceeded();
            if (s.size() > 16 * 1024 * 1024)
                invalid("字符串过长");
        }
        return s;
    }
    Json read(int depth,bool rows=false) {
        if(bounds_&&static_cast<std::size_t>(depth)>bounds_->depth)detail::JsonAllocationBounds::exceeded();
        if (depth > 72 || remaining_-- == 0)
            invalid("JSON 解析预算超限");
        space();
        if (pos_ == text_.size())
            invalid("缺少 JSON 值");
        char c = text_[pos_];
        if (c == '"')
            return Json(Json::Kind::String, quoted());
        if (c == '{' || c == '[') {
            pos_++;
            bool object = c == '{';
            char end = object ? '}' : ']';
            Json v(object ? Json::Kind::Object : Json::Kind::Array);
            if (consume(end))
                return v;
            do {
                if (object) {
                    if(bounds_&&v.members.size()>=bounds_->members)detail::JsonAllocationBounds::exceeded();
                    space();
                    auto name = quoted();
                    space();
                    expect(':');
                    bool is_rows=depth==0&&name==(bounds_?bounds_->root_array:"rows");
                    auto child=read(depth+1,is_rows);
                    if (!v.members.emplace(std::move(name), std::move(child)).second)
                        invalid("JSON 重复成员");
                } else {
                    if(bounds_&&v.items.size()>=(rows?bounds_->rows:bounds_->array))detail::JsonAllocationBounds::exceeded();
                    v.items.push_back(read(depth + 1));
                }
            } while (consume(','));
            space();
            expect(end);
            return v;
        }
        if (c == 't' || c == 'f' || c == 'n') {
            std::string token = c == 't' ? "true" : c == 'f' ? "false" : "null";
            for (char ch : token)
                expect(ch);
            return Json(c == 'n' ? Json::Kind::Null : Json::Kind::Bool, token);
        }
        auto start = pos_;
        if (c == '-')
            pos_++;
        if (pos_ == text_.size())
            invalid("JSON 数字无效");
        if (text_[pos_] == '0')
            pos_++;
        else {
            if (text_[pos_] < '1' || text_[pos_] > '9')
                invalid("JSON 数字无效");
            digits();
        }
        if (pos_ < text_.size() && text_[pos_] == '.') {
            pos_++;
            required_digits();
        }
        if (pos_ < text_.size() && (text_[pos_] == 'e' || text_[pos_] == 'E')) {
            pos_++;
            if (pos_ < text_.size() && (text_[pos_] == '+' || text_[pos_] == '-'))
                pos_++;
            required_digits();
        }
        if (pos_ - start > 1024)
            invalid("JSON 数字过长");
        return Json(Json::Kind::Number, text_.substr(start, pos_ - start));
    }

  public:
    explicit JsonParser(const std::string &text) : text_(text) {}
    JsonParser(const std::string& text,const detail::JsonAllocationBounds& bounds):text_(text),bounds_(&bounds){}
    Json parse() {
        if (!valid_utf8(text_))
            detail::decode_encoding();
        Json v = read(0);
        space();
        if (pos_ != text_.size())
            invalid("JSON 尾随内容");
        return v;
    }
};
inline Json parse_json(const std::string &text) { return JsonParser(text).parse(); }
template <class T> T integer(const Json &v) {
    const auto &s = sizeof(T) == 8 ? v.string() : v.number();
    if (s.empty() || s.find_first_not_of("-0123456789") != std::string::npos)
        invalid("整数格式无效");
    std::size_t end = 0;
    if constexpr (std::is_signed<T>::value) {
        auto n = std::stoll(s, &end);
        if (end != s.size() || n < (std::numeric_limits<T>::min)() || n > (std::numeric_limits<T>::max)())
            invalid("整数超出范围");
        return static_cast<T>(n);
    } else {
        if (s[0] == '-')
            invalid("无符号整数不能为负数");
        auto n = std::stoull(s, &end);
        if (end != s.size() || n > (std::numeric_limits<T>::max)())
            invalid("整数超出范围");
        return static_cast<T>(n);
    }
}
template <class T> T floating(const Json &v) {
    std::istringstream in(v.number());
    in.imbue(std::locale::classic());
    T n{};
    in >> n;
    if (!in || !in.eof() || !std::isfinite(n))
        invalid("浮点数无效");
    return n;
}

// decimal 保存规范化的精确十进制值，哈希在构造时缓存；查询键不会进行格式化和分配。
class Decimal {
    std::string canonical_ = "0";
    std::size_t hash_ = std::hash<std::string>{}("0");

  public:
    Decimal() = default;
    explicit Decimal(std::string s) {
        bool negative = !s.empty() && s[0] == '-';
        if (!s.empty() && (s[0] == '-' || s[0] == '+'))
            s.erase(0, 1);
        auto point = s.find('.');
        if (point == 0)
            invalid("decimal 必须包含整数部分");
        std::size_t scale = point == std::string::npos ? 0 : s.size() - point - 1;
        if (point != std::string::npos)
            s.erase(point, 1);
        if (s.empty() || s.find_first_not_of("0123456789") != std::string::npos)
            invalid("decimal 格式无效");
        auto nonzero = s.find_first_not_of('0');
        if (nonzero == std::string::npos)
            return;
        s.erase(0, nonzero);
        while (scale > 0 && s.back() == '0') {
            s.pop_back();
            scale--;
        }
        const std::string max = "79228162514264337593543950335";
        if (scale > 28 || s.size() > max.size() || (s.size() == max.size() && s > max))
            invalid("decimal 超出范围");
        if (scale > 0) {
            if (s.size() <= scale)
                s.insert(0, scale - s.size() + 1, '0');
            s.insert(s.size() - scale, 1, '.');
        }
        canonical_ = (negative ? "-" : "") + s;
        hash_ = std::hash<std::string>{}(canonical_);
    }
    const std::string &string() const noexcept { return canonical_; }
    std::size_t hash() const noexcept { return hash_; }
    bool operator==(const Decimal &other) const noexcept { return canonical_ == other.canonical_; }
};
template <class T> struct Hash {
    std::size_t operator()(const T &v) const noexcept { return std::hash<T>{}(v); }
};
template <> struct Hash<Decimal> {
    std::size_t operator()(const Decimal &v) const noexcept { return v.hash(); }
};
template <class T> class Matrix {
    std::vector<std::size_t> dimensions_;
    std::vector<T> values_;

  public:
    Matrix() = default;
    Matrix(std::vector<std::size_t> dimensions, std::vector<T> values)
        : dimensions_(std::move(dimensions)), values_(std::move(values)) {
        std::size_t n = 1;
        for (auto d : dimensions_) {
            if (d > max_elements || (d != 0 && n > max_elements / d))
                invalid("矩阵过大");
            n *= d;
        }
        if (n != values_.size())
            invalid("矩阵长度不匹配");
    }
    std::size_t rank() const noexcept { return dimensions_.size(); }
    std::size_t dimension(std::size_t axis) const { return dimensions_.at(axis); }
    const std::vector<T> &values() const noexcept { return values_; }
};
template <class F> auto decode_list(const Json &v, F decode) {
    using T = decltype(decode(v));
    const auto &source = v.array();
    if (source.size() > max_elements)
        invalid("数组过长");
    std::vector<T> values;
    values.reserve(source.size());
    for (const auto &x : source)
        values.push_back(decode(x));
    return values;
}
template <class F> auto decode_optional(const Json &v, F decode) {
    using T = decltype(decode(v));
    return v.kind == Json::Kind::Null ? std::optional<T>{} : std::optional<T>{decode(v)};
}
template <class K, class V> auto decode_dictionary(const Json &v, K key, V value) {
    using Key = decltype(key(v));
    using Value = decltype(value(v));
    std::unordered_map<Key, Value, Hash<Key>> result;
    result.reserve(v.array().size());
    for (const auto &x : v.array()) {
        const auto &pair = x.array();
        if (pair.size() != 2)
            invalid("字典条目无效");
        if (!result.emplace(key(pair[0]), value(pair[1])).second)
            invalid("字典键重复");
    }
    return result;
}
template <class F> auto decode_matrix(const Json &v, F decode) {
    using T = decltype(decode(v));
    std::vector<std::size_t> dims;
    for (const auto &d : v.field("dimensions").array()) {
        auto n = integer<std::int32_t>(d);
        if (n < 0)
            invalid("矩阵维度无效");
        dims.push_back(static_cast<std::size_t>(n));
    }
    return Matrix<T>(std::move(dims), decode_list(v.field("values"), decode));
}

// 开放寻址索引保存行号；查询比较原始键分量，不创建组合键或复制字符串。
template <class Row> class Index {
  protected:
    static constexpr std::size_t missing = (std::numeric_limits<std::size_t>::max)();
    const std::vector<Row> &rows_;
    std::vector<std::size_t> slots_;
    std::size_t mask_;
    std::vector<std::vector<const Row *>> groups_;
    static std::size_t mix(std::size_t x) {
        x ^= x >> 16;
        x *= 0x7feb352dU;
        x ^= x >> 15;
        return x;
    }

  public:
    template <class H, class E>
    Index(const std::vector<Row> &rows, bool unique, H hash, E equal) : rows_(rows), mask_(1) {
        while (mask_ < rows.size() * 2)
            mask_ <<= 1;
        slots_.assign(mask_, missing);
        if (!unique)
            groups_.resize(mask_);
        --mask_;
        for (std::size_t i = 0; i < rows.size(); ++i) {
            auto slot = mix(hash(rows[i])) & mask_;
            while (slots_[slot] != missing && !equal(rows[slots_[slot]], rows[i]))
                slot = (slot + 1) & mask_;
            if (slots_[slot] != missing && unique)
                invalid("主键重复");
            if (slots_[slot] == missing)
                slots_[slot] = i;
            if (!unique)
                groups_[slot].push_back(&rows[i]);
        }
    }
    const Row *row(std::size_t slot) const noexcept { return slot == missing ? nullptr : &rows_[slots_[slot]]; }
    const std::vector<const Row *> &group(std::size_t slot) const {
        static const std::vector<const Row *> empty;
        return slot == missing ? empty : groups_[slot];
    }
};

inline std::uint32_t rotate(std::uint32_t x, int n) { return (x >> n) | (x << (32 - n)); }
// SHA-256 仅校验文件完整性；不将该哈希当作身份认证或签名。
inline std::string sha256(const std::vector<std::uint8_t> &bytes) {
    static constexpr std::uint32_t k[] = {
        0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
        0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
        0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
        0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
        0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
        0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
        0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
        0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2};
    std::uint32_t h[] = {0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a,
                         0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19};
    std::size_t padded = ((bytes.size() + 9 + 63) / 64) * 64;
    for (std::size_t start = 0; start < padded; start += 64) {
        std::uint32_t w[64]{};
        for (std::size_t i = 0; i < 64; i++) {
            std::size_t p = start + i;
            std::uint8_t b = 0;
            if (p < bytes.size())
                b = bytes[p];
            else if (p == bytes.size())
                b = 0x80;
            else if (p >= padded - 8)
                b = static_cast<std::uint8_t>((static_cast<std::uint64_t>(bytes.size()) * 8) >> ((padded - 1 - p) * 8));
            w[i / 4] |= static_cast<std::uint32_t>(b) << (24 - (i % 4) * 8);
        }
        for (int i = 16; i < 64; i++) {
            auto a = w[i - 15], b = w[i - 2];
            w[i] = w[i - 16] + (rotate(a, 7) ^ rotate(a, 18) ^ (a >> 3)) + w[i - 7] +
                   (rotate(b, 17) ^ rotate(b, 19) ^ (b >> 10));
        }
        auto a = h[0], b = h[1], c = h[2], d = h[3], e = h[4], f = h[5], g = h[6], j = h[7];
        for (int i = 0; i < 64; i++) {
            auto t1 = j + (rotate(e, 6) ^ rotate(e, 11) ^ rotate(e, 25)) + ((e & f) ^ (~e & g)) + k[i] + w[i];
            auto t2 = (rotate(a, 2) ^ rotate(a, 13) ^ rotate(a, 22)) + ((a & b) ^ (a & c) ^ (b & c));
            j = g;
            g = f;
            f = e;
            e = d + t1;
            d = c;
            c = b;
            b = a;
            a = t1 + t2;
        }
        h[0] += a;
        h[1] += b;
        h[2] += c;
        h[3] += d;
        h[4] += e;
        h[5] += f;
        h[6] += g;
        h[7] += j;
    }
    std::ostringstream out;
    out << std::hex << std::setfill('0');
    for (auto n : h)
        out << std::setw(8) << n;
    return out.str();
}
struct Cursor {
    const std::vector<std::uint8_t> &data;
    std::size_t pos = 0;
    std::uint64_t bits(int size) {
        if (static_cast<std::size_t>(size) > data.size() - pos)
            invalid("二进制数据截断");
        std::uint64_t v = 0;
        for (int i = 0; i < size; i++)
            v |= static_cast<std::uint64_t>(data[pos++]) << (i * 8);
        return v;
    }
    std::int32_t i32() {
        auto u = static_cast<std::uint32_t>(bits(4));
        std::int32_t v;
        std::memcpy(&v, &u, 4);
        return v;
    }
    std::size_t count(std::size_t max = max_elements) {
        auto n = i32();
        if (n < 0 || static_cast<std::size_t>(n) > max)
            invalid("长度无效");
        return static_cast<std::size_t>(n);
    }
    bool boolean() {
        auto n = bits(1);
        if (n > 1)
            invalid("bool 编码无效");
        return n != 0;
    }
    std::string text() {
        auto n = i32();
        if (n == -1)
            return "";
        if (n < 0 || n > 16 * 1024 * 1024 || static_cast<std::size_t>(n) > data.size() - pos)
            invalid("字符串长度无效");
        if(static_cast<std::size_t>(n)>detail::decode_string_bytes())detail::decode_limit("String byte limit exceeded");
        std::string s(reinterpret_cast<const char *>(data.data() + pos), n);
        pos += n;
        if (!valid_utf8(s))
            detail::decode_encoding();
        return s;
    }
};
class Wire {
    friend class detail::IndexedRows;
    Json schema_;
    std::unordered_map<std::string, const Json *> types_;
    std::vector<std::string> tags_;
    inline static thread_local std::size_t remaining_ = detail::decode_elements();
    static bool contains(const Json &values, const std::string &key) {
        for (const auto &v : values.array())
            if (v.string() == key)
                return true;
        return false;
    }
    void claim(std::size_t count) {
        if (count > max_elements || count > remaining_)
            detail::decode_limit("元素预算超限");
        remaining_ -= count;
    }
    Json fields(const Json &fs, Cursor &r, int depth) {
        Json v(Json::Kind::Object);
        for(const auto& f:fs.array())try{v.members.emplace(f.field("name").string(),value(f.field("type").string(),r,depth+1));}catch(...){detail::field_failure(f.field("name").string());}
        return v;
    }
    Json primitive(const std::string &kind, Cursor &r) {
        if (kind == "string")
            return Json(Json::Kind::String, r.text());
        if (kind == "bool")
            return Json(Json::Kind::Bool, r.boolean() ? "true" : "false");
        if (kind == "decimal") {
            std::array<std::uint32_t, 3> words{static_cast<std::uint32_t>(r.bits(4)),
                                               static_cast<std::uint32_t>(r.bits(4)),
                                               static_cast<std::uint32_t>(r.bits(4))};
            auto flags = static_cast<std::uint32_t>(r.bits(4));
            auto scale = (flags >> 16) & 255;
            if ((flags & 0x7f00ffff) != 0 || scale > 28)
                invalid("decimal 编码无效");
            std::string digits;
            do {
                std::uint64_t carry = 0;
                for (int i = 2; i >= 0; i--) {
                    auto n = (carry << 32) | words[i];
                    words[i] = static_cast<std::uint32_t>(n / 10);
                    carry = n % 10;
                }
                digits.push_back(static_cast<char>('0' + carry));
            } while (words[0] || words[1] || words[2]);
            std::reverse(digits.begin(), digits.end());
            if (scale) {
                if (digits.size() <= scale)
                    digits.insert(0, scale - digits.size() + 1, '0');
                digits.insert(digits.size() - scale, 1, '.');
            }
            if (flags >> 31)
                digits.insert(0, 1, '-');
            return Json(Json::Kind::String, digits);
        }
        if (kind == "float32" || kind == "float64") {
            std::ostringstream out;
            out.imbue(std::locale::classic());
            if (kind == "float32") {
                auto bits = static_cast<std::uint32_t>(r.bits(4));
                float f;
                std::memcpy(&f, &bits, 4);
                if (!std::isfinite(f))
                    invalid("浮点数无效");
                out << std::setprecision(std::numeric_limits<float>::max_digits10) << f;
            } else {
                auto bits = r.bits(8);
                double d;
                std::memcpy(&d, &bits, 8);
                if (!std::isfinite(d))
                    invalid("浮点数无效");
                out << std::setprecision(std::numeric_limits<double>::max_digits10) << d;
            }
            return Json(Json::Kind::Number, out.str());
        }
        bool is_unsigned = kind.rfind("uint", 0) == 0;
        int width = std::stoi(kind.substr(is_unsigned ? 4 : 3));
        auto bits = r.bits(width / 8);
        std::string text;
        if (is_unsigned)
            text = std::to_string(bits);
        else {
            if (width < 64 && (bits & (std::uint64_t(1) << (width - 1))))
                bits |= (~std::uint64_t(0)) << width;
            std::int64_t n;
            std::memcpy(&n, &bits, 8);
            text = std::to_string(n);
        }
        return Json(width == 64 ? Json::Kind::String : Json::Kind::Number, text);
    }
    Json nested_read(const std::string& id,Cursor& r,int depth,const std::string& path){try{return value(id,r,depth);}catch(...){detail::field_failure(path);}}
    void nested_validate(const std::string& id,const Json& v,int depth,const std::string& path){try{validate(id,v,depth);}catch(...){detail::field_failure(path);}}
    Json value(const std::string &id, Cursor &r, int depth) {
        if (static_cast<std::size_t>(depth) > detail::decode_depth())
            detail::decode_limit("嵌套过深");
        const auto &t = *types_.at(id);
        auto kind = t.field("kind").string();
        if (kind == "primitive" || kind == "enum" || kind == "tablereference") {
            auto v = primitive(t.field("primitive").string(), r);
            if (kind == "enum" && !contains(t.field("enumValues"), v.text))
                invalid("未知枚举值");
            return v;
        }
        if (kind == "rawjson")
            return Json(Json::Kind::String, r.text());
        if (kind == "nullable")
            return r.boolean() ? value(t.field("element").string(), r, depth + 1) : Json{};
        if (kind == "array") {
            auto rank = integer<std::int32_t>(t.field("rank"));
            Json dims(Json::Kind::Array);
            std::size_t count = 1;
            for (int i = 0; i < rank; i++) {
                auto n = r.count();
                if (n != 0 && count > max_elements / n)
                    invalid("矩阵过大");
                count *= n;
                dims.items.emplace_back(Json::Kind::Number, std::to_string(n));
            }
            claim(count);
            Json values(Json::Kind::Array);
            values.items.reserve(count);
            for (std::size_t i = 0; i < count; i++)
                values.items.push_back(nested_read(t.field("element").string(),r,depth+1,"["+std::to_string(i)+"]"));
            if (rank == 1)
                return values;
            Json matrix(Json::Kind::Object);
            matrix.members.emplace("dimensions", std::move(dims));
            matrix.members.emplace("values", std::move(values));
            return matrix;
        }
        if (kind == "dictionary") {
            auto n = r.count();
            claim(n);
            Json values(Json::Kind::Array);
            values.items.reserve(n);
            for (std::size_t i = 0; i < n; i++) {
                Json pair(Json::Kind::Array);
                pair.items.push_back(nested_read(t.field("key").string(),r,depth+1,"["+std::to_string(i)+"].key"));
                pair.items.push_back(nested_read(t.field("element").string(),r,depth+1,"["+std::to_string(i)+"].value"));
                values.items.push_back(std::move(pair));
            }
            return values;
        }
        if (kind == "object" && !t.field("variants").array().empty()) {
            auto index = r.i32();
            if (index == -1)
                return Json{};
            if (index < 0 || static_cast<std::size_t>(index) >= tags_.size() ||
                !contains(t.field("variants"), tags_[index]))
                invalid("多态类型不匹配");
            auto v = fields(types_.at(tags_[index])->field("fields"), r, depth);
            v.members.emplace("$type", Json(Json::Kind::String, tags_[index]));
            return v;
        }
        if (t.field("nullableObject").boolean() && !r.boolean())
            return Json{};
        return fields(t.field("fields"), r, depth);
    }
    void validate_fields(const Json &fs, const Json &v, int depth, bool tagged) {
        if (v.kind != Json::Kind::Object || v.members.size() != fs.array().size() + (tagged ? 1 : 0))
            invalid("未知或缺失成员");
        for(const auto& f:fs.array())try{validate(f.field("type").string(),v.field(f.field("name").string()),depth+1);}catch(...){detail::field_failure(f.field("name").string());}
    }
    void validate(const std::string &id, const Json &v, int depth) {
        if (static_cast<std::size_t>(depth) > detail::decode_depth())
            detail::decode_limit("嵌套过深");
        const auto &t = *types_.at(id);
        auto kind = t.field("kind").string();
        if (kind == "primitive" || kind == "enum" || kind == "tablereference") {
            auto p = t.field("primitive").string();
            if (p == "string") {
                if(v.string().size()>detail::decode_string_bytes())detail::decode_limit("String byte limit exceeded");
            }
            else if (p == "bool")
                v.boolean();
            else if (p == "int8")
                integer<std::int8_t>(v);
            else if (p == "uint8")
                integer<std::uint8_t>(v);
            else if (p == "int16")
                integer<std::int16_t>(v);
            else if (p == "uint16")
                integer<std::uint16_t>(v);
            else if (p == "int32")
                integer<std::int32_t>(v);
            else if (p == "uint32")
                integer<std::uint32_t>(v);
            else if (p == "int64")
                integer<std::int64_t>(v);
            else if (p == "uint64")
                integer<std::uint64_t>(v);
            else if (p == "float32")
                floating<float>(v);
            else if (p == "float64")
                floating<double>(v);
            else if (p == "decimal") {
                Decimal d(v.string());
                (void)d;
            } else
                invalid("未知标量");
            if (kind == "enum" && !contains(t.field("enumValues"), v.text))
                invalid("未知枚举值");
            return;
        }
        if (kind == "rawjson") {
            if(v.string().size()>detail::decode_string_bytes())detail::decode_limit("String byte limit exceeded");
            parse_json(v.string());
            return;
        }
        if (kind == "nullable") {
            if (v.kind != Json::Kind::Null)
                validate(t.field("element").string(), v, depth + 1);
            return;
        }
        if (kind == "array") {
            auto rank = integer<std::int32_t>(t.field("rank"));
            const auto &values = (rank == 1 ? v : v.field("values")).array();
            claim(values.size());
            if (rank > 1) {
                const auto &dims = v.field("dimensions").array();
                if (dims.size() != static_cast<std::size_t>(rank) || v.members.size() != 2)
                    invalid("矩阵维度错误");
                std::size_t n = 1;
                for (const auto &d : dims) {
                    auto x = integer<std::int32_t>(d);
                    if (x < 0 || static_cast<std::size_t>(x) > max_elements ||
                        (x != 0 && n > max_elements / static_cast<std::size_t>(x)))
                        invalid("矩阵过大");
                    n *= x;
                }
                if (n != values.size())
                    invalid("矩阵长度不匹配");
            }
            for (std::size_t i=0;i<values.size();++i)nested_validate(t.field("element").string(),values[i],depth+1,"["+std::to_string(i)+"]");
            return;
        }
        if (kind == "dictionary") {
            claim(v.array().size());
            for (std::size_t i=0;i<v.array().size();++i) {
                const auto& x=v.items[i];
                if (x.array().size() != 2)
                    invalid("字典条目错误");
                nested_validate(t.field("key").string(),x.items[0],depth+1,"["+std::to_string(i)+"].key");
                nested_validate(t.field("element").string(),x.items[1],depth+1,"["+std::to_string(i)+"].value");
            }
            return;
        }
        if (v.kind == Json::Kind::Null && t.field("nullableObject").boolean())
            return;
        if (!t.field("variants").array().empty()) {
            auto tag = v.field("$type").string();
            if (!contains(t.field("variants"), tag))
                invalid("多态类型错误");
            validate_fields(types_.at(tag)->field("fields"), v, depth, true);
        } else
            validate_fields(t.field("fields"), v, depth, false);
    }

  public:
    explicit Wire(const std::string &schema) : schema_(parse_json(schema)) {
        for (const auto &type : schema_.field("types").array())
            types_.emplace(type.field("id").string(), &type);
    }
    const std::string &name() const { return schema_.field("name").string(); }
    const std::string &hash() const { return schema_.field("hash").string(); }
    std::vector<Json> read(const std::vector<std::uint8_t> &bytes, bool json) {
        if (json) {
            auto doc = parse_json(std::string(bytes.begin(), bytes.end()));
            if (doc.members.size() != 4 || doc.field("format").string() != "ZrConfig.TableJson/1" ||
                doc.field("table").string() != name() || doc.field("schemaHash").string() != hash())
                invalid("JSON 表头不匹配");
            auto rows = std::move(doc.members.at("rows"));
            if (rows.array().size() > max_rows)
                invalid("行数过大");
            for (const auto &row : rows.items) {
                remaining_ = detail::decode_elements();
                validate_fields(schema_.field("fields"), row, 0, false);
            }
            return std::move(rows.items);
        }
        Cursor r{bytes};
        if (r.i32() != 0x4643525a || r.i32() != 4 || r.text() != name() || r.text() != hash())
            invalid("二进制表头不匹配");
        auto count = r.count(max_rows), nt = r.count(schema_.field("tags").array().size());
        if (count > bytes.size() / 12)
            invalid("行索引长度不足");
        std::unordered_set<std::string> allowed;
        for (const auto &v : schema_.field("tags").array())
            allowed.insert(v.string());
        std::unordered_set<std::string> seen;
        for (std::size_t i = 0; i < nt; i++) {
            auto tag = r.text();
            if (!allowed.count(tag) || !seen.insert(tag).second)
                invalid("类型映射无效");
            tags_.push_back(tag);
        }
        std::vector<std::int32_t> offsets(count), lengths(count), ids(count);
        std::vector<std::vector<Json>> keys(count);
        const auto &key_fields = schema_.field("keys").array();
        for (std::size_t i = 0; i < count; i++) {
            remaining_ = detail::decode_elements();
            for (const auto &key : key_fields)
                for (const auto &f : schema_.field("fields").array())
                    if (f.field("name").string() == key.string())
                        keys[i].push_back(value(f.field("type").string(), r, 0));
            ids[i] = r.i32();
            offsets[i] = r.i32();
            lengths[i] = r.i32();
            if (offsets[i] < 0 || lengths[i] < 0 ||
                (i == 0 ? offsets[i] != 0 : static_cast<std::int64_t>(offsets[i - 1]) + lengths[i - 1] != offsets[i]))
                invalid("行索引不连续");
        }
        auto start = r.pos;
        std::uint64_t end = start + (count == 0 ? 0 : static_cast<std::uint64_t>(offsets.back()) + lengths.back());
        if (end != bytes.size())
            invalid("文件长度不匹配");
        std::vector<Json> rows;
        rows.reserve(count);
        for (std::size_t i = 0; i < count; i++) {
            remaining_ = detail::decode_elements();
            auto row = fields(schema_.field("fields"), r, 0);
            if (r.pos != start + static_cast<std::uint64_t>(offsets[i]) + lengths[i])
                invalid("行边界不匹配");
            for (std::size_t k = 0; k < key_fields.size(); k++) {
                const auto &v = row.field(key_fields[k].string());
                if (v.kind != keys[i][k].kind || v.text != keys[i][k].text)
                    invalid("行主键与索引不匹配");
            }
            if (ids[i] != (schema_.field("hasId").boolean() ? integer<std::int32_t>(row.field("Id")) : 0))
                invalid("行 Id 不匹配");
            rows.push_back(std::move(row));
        }
        return rows;
    }
};
inline std::vector<std::uint8_t> read_file(const std::filesystem::path &path) {
    std::ifstream in(path, std::ios::binary);
    if (!in)
        invalid("无法读取文件：" + path.string());
    in.seekg(0, std::ios::end);
    auto size = in.tellg();
    if (size < 0 || size > (std::numeric_limits<std::int32_t>::max)())
        invalid("文件过大");
    in.seekg(0);
    std::vector<std::uint8_t> data(static_cast<std::size_t>(size));
    if (!data.empty() && !in.read(reinterpret_cast<char *>(data.data()), static_cast<std::streamsize>(data.size())))
        invalid("文件读取不完整");
    return data;
}
inline std::string path_segment(const std::string &name) {
    if (name.empty() || name[0] == '.' || name.find_first_of("/\\:") != std::string::npos)
        invalid("清单路径无效");
    return name;
}
class Bundle {
    friend class Catalog;
    std::string target_;
    friend class detail::IndexedRows;
    friend class detail::SourceBundle;
    std::filesystem::path folder_;
    std::unordered_map<std::string, Json> entries_;

  public:
    std::string version;
    explicit Bundle(const std::filesystem::path &root) {
        auto bytes = read_file(root / "current.json");
        initialize(root,parse_json(std::string(bytes.begin(), bytes.end())));
    }
    Bundle(const std::filesystem::path& root,const Json& manifest){initialize(root,manifest);}
private:
    void initialize(const std::filesystem::path& root,const Json& manifest){
        auto target=manifest.members.find("target");target_=target!=manifest.members.end()&&target->second.kind==Json::Kind::String?target->second.text:"";
        auto format = manifest.field("format").string();
        if (format != "ZrConfig.Binary/1" && format != "ZrConfig.Json/1" && format != "ZrConfig.Mixed/1")
            invalid("清单格式不支持");
        version = path_segment(manifest.field("versionId").string());
        folder_ = root / "versions" / version;
        for (const auto &entry : manifest.field("tables").array()) {
            auto name = path_segment(entry.field("name").string());
            auto rows = integer<std::int32_t>(entry.field("rows"));
            if (entry.field("file").string() != name + (format == "ZrConfig.Json/1" ? ".json" : ".bytes") || rows < 0 ||
                static_cast<std::size_t>(rows) > max_rows)
                invalid("清单文件名或行数错误");
            auto valid_hash = [](const std::string &value) {
                return value.size() == 64 && value.find_first_not_of("0123456789abcdefABCDEF") == std::string::npos;
            };
            if (!valid_hash(entry.field("sha256").string()) || !valid_hash(entry.field("schemaHash").string()))
                invalid("清单哈希无效");
            if (format == "ZrConfig.Mixed/1") {
                if (entry.field("jsonFile").string() != name + ".json" ||
                    !valid_hash(entry.field("jsonSha256").string()))
                    invalid("JSON 清单无效");
            } else if ((entry.members.count("jsonFile") && entry.field("jsonFile").kind != Json::Kind::Null) ||
                       (entry.members.count("jsonSha256") && entry.field("jsonSha256").kind != Json::Kind::Null))
                invalid("格式与 JSON 清单不一致");
            if (!entries_.emplace(name, entry).second)
                invalid("清单表重复");
        }
    }
public:
    void validate_tables(const std::vector<std::string> &expected) const {
        if (entries_.size() != expected.size()) invalid("清单表集合不匹配");
        for (const auto &name : expected)
            if (!entries_.count(name)) invalid("清单表集合不匹配");
    }
    bool matches_schema(const std::string& name, const std::string& hash) const {
        auto entry = entries_.find(name);
        return entry != entries_.end() && entry->second.field("schemaHash").string() == hash;
    }
    std::vector<Json> load(const std::string &schema, bool prefer_json) {
        Wire wire(schema);
        auto e = entries_.find(wire.name());
        if (e == entries_.end())
            invalid("清单缺少表：" + wire.name());
        const auto &entry = e->second;
        if (entry.field("schemaHash").string() != wire.hash())
            invalid("表结构不匹配");
        bool alternate =
            prefer_json && entry.members.count("jsonFile") && entry.field("jsonFile").kind == Json::Kind::String;
        auto file = path_segment(entry.field(alternate ? "jsonFile" : "file").string());
        auto bytes = read_file(folder_ / file);
        auto expected = entry.field(alternate ? "jsonSha256" : "sha256").string();
        std::transform(expected.begin(), expected.end(), expected.begin(),
                       [](unsigned char c) { return static_cast<char>(std::tolower(c)); });
        if (sha256(bytes) != expected)
            invalid("文件哈希不匹配");
        auto rows = wire.read(bytes, file.size() >= 5 && file.substr(file.size() - 5) == ".json");
        if (rows.size() != static_cast<std::size_t>(integer<std::int32_t>(entry.field("rows"))))
            invalid("清单行数不匹配");
        return rows;
    }
};
struct TableDescriptor {
    std::string name, schema_hash;
    std::vector<std::string> dependencies;
    std::size_t component;
};
struct SelectionPlan {
    std::vector<std::string> requested, tables;
    std::string catalog_fingerprint;
};
class Catalog {
    std::string fingerprint_,target_;
    std::vector<TableDescriptor> tables_;
    std::unordered_map<std::string, std::size_t> names_;
    std::unordered_map<std::size_t, std::vector<std::size_t>> components_;
public:
    explicit Catalog(const std::string& source) {
        auto definition = parse_json(source);
        auto format=integer<int>(definition.field("format"));if(format!=1&&format!=2)throw ConfigError(ErrorCode::InvalidPlan,"Unknown catalog format");
        if(format==2){const std::unordered_set<std::string> supported{"binary-v4", "json-v1", "server-default", "selected-closure", "lazy-strict", "lazy-on-access", "fixed-source-bytes", "snapshot-owned-handles", "explicit-reference-load", "row-cache", "group-cache", "typed-warmup", "bounded-source", "structured-errors", "manager-lifecycle"};if(definition.field("runtimeContract").string()!="server-v1")throw ConfigError(ErrorCode::UnsupportedCapability,"Unsupported runtime contract");for(const auto& value:definition.field("requiredCapabilities").array())if(!supported.count(value.string()))throw ConfigError(ErrorCode::UnsupportedCapability,"Unsupported runtime capability: "+value.string());}
        auto target=definition.members.find("target");target_=target!=definition.members.end()&&target->second.kind==Json::Kind::String?target->second.text:"";
        fingerprint_ = definition.field("fingerprint").string();
        for (const auto& value : definition.field("tables").array()) {
            TableDescriptor table{value.field("name").string(), value.field("schemaHash").string(), {}, integer<std::uint32_t>(value.field("component"))};
            for (const auto& dependency : value.field("dependencies").array()) table.dependencies.push_back(dependency.string());
            if (!names_.emplace(table.name, tables_.size()).second) throw ConfigError(ErrorCode::InvalidPlan, "Duplicate catalog table");
            components_[table.component].push_back(tables_.size()); tables_.push_back(std::move(table));
        }
        for (const auto& table : tables_) for (const auto& name : table.dependencies)
            if (!names_.count(name)) throw ConfigError(ErrorCode::InvalidPlan, "Unknown catalog dependency");
    }
    const std::string& fingerprint() const noexcept { return fingerprint_; }
    const std::vector<TableDescriptor>& tables() const noexcept { return tables_; }
    SelectionPlan select(const std::vector<std::string>& requested) const {
        SelectionPlan plan{requested, {}, fingerprint_};
        std::vector<std::string> queue = requested;
        std::unordered_set<std::size_t> visited, selected;
        for (std::size_t i = 0; i < queue.size(); ++i) {
            auto found = names_.find(queue[i]); if (found == names_.end()) throw ConfigError(ErrorCode::InvalidPlan, "Unknown selected table: " + queue[i]);
            auto component = tables_[found->second].component;
            if (!visited.insert(component).second) continue;
            for (auto member : components_.at(component)) {
                selected.insert(member); const auto& dependencies = tables_[member].dependencies;
                queue.insert(queue.end(), dependencies.begin(), dependencies.end());
            }
        }
        for (std::size_t i = 0; i < tables_.size(); ++i) if (selected.count(i)) plan.tables.push_back(tables_[i].name);
        std::sort(plan.requested.begin(), plan.requested.end()); return plan;
    }
    void validate(const Bundle& bundle) const {
        if(!target_.empty()&&target_!=bundle.target_)throw ConfigError(ErrorCode::CatalogMismatch,"Manifest target differs from catalog","manifest");
        std::vector<std::string> names; names.reserve(tables_.size()); for (const auto& table : tables_) names.push_back(table.name);
        try { bundle.validate_tables(names); } catch (const detail::InvalidData&) { throw ConfigError(ErrorCode::TableSetMismatch, "Manifest table set differs from catalog", "manifest"); }
        for (const auto& table : tables_) if (!bundle.matches_schema(table.name, table.schema_hash)) throw ConfigError(ErrorCode::SchemaMismatch, "Schema mismatch: " + table.name, "manifest");
    }
};
enum class LoadMode { ServerDefault, SelectedTables, LazyRowsStrict, LazyRowsOnAccess };
namespace detail {
inline std::string load_plan_fingerprint(LoadMode mode,bool json,const std::vector<std::string>& tables){
    std::string name;switch(mode){case LoadMode::ServerDefault:name="ServerDefault";break;case LoadMode::SelectedTables:name="SelectedTables";break;case LoadMode::LazyRowsStrict:name="LazyRowsStrict";break;case LoadMode::LazyRowsOnAccess:name="LazyRowsOnAccess";break;default:throw ConfigError(ErrorCode::InvalidPlan,"Invalid mode");}
    const char* digits="0123456789abcdef";std::vector<std::string> names;for(const auto& table:tables){std::string encoded;for(unsigned char byte:table){encoded.push_back(digits[byte>>4]);encoded.push_back(digits[byte&15]);}names.push_back(std::move(encoded));}std::sort(names.begin(),names.end());
    std::string value="zrconfig-load-plan/1\n"+name+"\n"+(json?"json-preferred":"binary")+"\n";for(std::size_t i=0;i<names.size();++i){if(i)value+='\n';value+=names[i];}return sha256(std::vector<std::uint8_t>(value.begin(),value.end()));
}}

struct LoadPlan {
    LoadMode mode = LoadMode::ServerDefault;
    std::optional<std::vector<std::string>> tables;
    bool prefer_json = false;
    std::vector<std::string> warmup_tables;
};
class LoadSettings {
    LoadPlan plan_;ResourceLimits limits_;std::string namespace_;
public:
    explicit LoadSettings(LoadPlan plan={}):plan_(std::move(plan)){}
    LoadSettings& with_limits(ResourceLimits value){limits_=value;return *this;}
    LoadSettings& with_source_namespace(std::string value){namespace_=std::move(value);return *this;}
    const LoadPlan& plan()const noexcept{return plan_;}
    const ResourceLimits& limits()const noexcept{return limits_;}
    const std::string& source_namespace()const noexcept{return namespace_;}
};
namespace detail {
class SourceBundle {
    ConfigSource& source_;const Cancellation& cancellation_;std::shared_ptr<LoadBudget> budget_;std::string digest_;std::unique_ptr<Bundle> metadata_;
    std::vector<std::uint8_t> capture(const std::string& path,std::uint64_t maximum){
        try{return source_.read(path,maximum,cancellation_);}catch(const ConfigError& failure){
            ErrorContext context;context.relative_path=path;auto first=path.find('/'),last=path.rfind('/');
            if(first!=std::string::npos&&last>first){context.version_id=path.substr(first+1,last-first-1);context.table=path.substr(last+1);auto dot=context.table.rfind('.');if(dot!=std::string::npos)context.table.resize(dot);}
            throw failure.locate("read",std::move(context));
        }
    }
public:
    SourceBundle(ConfigSource& source,const LoadSettings& settings,const Cancellation& cancellation):source_(source),cancellation_(cancellation),budget_(std::make_shared<LoadBudget>(settings.limits())){
        auto maximum=(std::min)(settings.limits().max_manifest_bytes(),settings.limits().max_bundle_bytes());auto bytes=capture("current.json",maximum);cancellation.check();if(bytes.size()>maximum)throw ConfigError(ErrorCode::LimitExceeded,"Source exceeded read limit");budget_->read(bytes.size());digest_=sha256(bytes);
        std::string text(bytes.begin(),bytes.end());if(!valid_utf8(text))throw ConfigError(ErrorCode::InvalidEncoding,"Invalid manifest UTF-8");
        try{auto manifest=JsonParser(text,JsonAllocationBounds(settings.limits())).parse();const auto& format=manifest.field("format").string();if(format.empty())throw ConfigError(ErrorCode::InvalidManifest,"Empty manifest format");if(format!="ZrConfig.Binary/1"&&format!="ZrConfig.Json/1"&&format!="ZrConfig.Mixed/1")throw ConfigError(ErrorCode::UnsupportedFormat,"Unknown manifest format");if(manifest.field("tables").array().size()>settings.limits().max_tables())throw ConfigError(ErrorCode::LimitExceeded,"Too many manifest tables");metadata_=std::make_unique<Bundle>(std::filesystem::path{},manifest);}catch(const InvalidData& error){throw ConfigError(ErrorCode::InvalidManifest,error.what());}
    }
    const Bundle& metadata()const noexcept{return *metadata_;}
    const std::string& version()const noexcept{return metadata_->version;}
    const std::string& manifest_digest()const noexcept{return digest_;}
    std::shared_ptr<LoadBudget> budget()const noexcept{return budget_;}
    const Json& entry(const std::string& name)const{auto found=metadata_->entries_.find(name);if(found==metadata_->entries_.end())throw ConfigError(ErrorCode::SchemaMismatch,"Missing table");return found->second;}
    std::vector<std::uint8_t> read(const std::string& file){auto maximum=budget_->read_limit();auto bytes=capture("versions/"+version()+"/"+file,maximum);cancellation_.check();if(bytes.size()>maximum)throw ConfigError(ErrorCode::LimitExceeded,"Source exceeded read limit");budget_->read(bytes.size());return bytes;}
};
}
namespace detail {
template<class Key>class OrdinalIndex {
protected:
    static constexpr std::size_t missing=(std::numeric_limits<std::size_t>::max)();
    std::vector<Key> keys_;
    std::vector<std::size_t> slots_;
    std::vector<std::vector<std::size_t>> groups_;
    std::size_t mask_=1;
    static std::size_t mix(std::size_t x){x^=x>>16;x*=0x7feb352dU;x^=x>>15;return x;}
public:
    template<class H,class E>OrdinalIndex(std::vector<Key> keys,bool unique,H hash,E equal):keys_(std::move(keys)){
        while(mask_<keys_.size()*2)mask_<<=1;
        slots_.assign(mask_,missing);
        if(!unique)groups_.resize(mask_);
        --mask_;
        for(std::size_t i=0;i<keys_.size();++i){auto slot=mix(hash(keys_[i]))&mask_;while(slots_[slot]!=missing&&!equal(keys_[slots_[slot]],keys_[i]))slot=(slot+1)&mask_;if(unique&&slots_[slot]!=missing)throw ConfigError(ErrorCode::InvalidValue,"Duplicate key");if(slots_[slot]==missing)slots_[slot]=i;if(!unique)groups_[slot].push_back(i);}
    }
    std::size_t ordinal(std::size_t slot)const noexcept{return slot==missing?missing:slots_[slot];}
    const std::vector<std::size_t>& group(std::size_t slot)const{static const std::vector<std::size_t> empty;return slot==missing?empty:groups_[slot];}
};
inline void append_json(std::string& output, const Json& value) {
    switch(value.kind) {
    case Json::Kind::Null: output += "null"; return;
    case Json::Kind::Bool: case Json::Kind::Number: output += value.text; return;
    case Json::Kind::String:
        output += '"'; for(unsigned char c : value.text) { if(c=='"'||c=='\\') { output+='\\';output+=static_cast<char>(c); }
            else if(c<32) { const char* hex="0123456789abcdef";output+="\\u00";output+=hex[c>>4];output+=hex[c&15]; } else output+=static_cast<char>(c); } output+='"';return;
    case Json::Kind::Array:
        output+='[';for(std::size_t i=0;i<value.items.size();++i){if(i)output+=',';append_json(output,value.items[i]);}output+=']';return;
    case Json::Kind::Object:
        output+='{';bool comma=false;for(const auto& member:value.members){if(comma)output+=',';comma=true;append_json(output,Json(Json::Kind::String,member.first));output+=':';append_json(output,member.second);}output+='}';return;
    }
}
class IndexedRows {
    std::uint64_t index_raw_bytes_=0;
    std::string version_,relative_path_;
    Wire wire_;
    JsonAllocationBounds json_bounds_;
    std::vector<std::uint8_t> bytes_;
    std::vector<std::string> fragments_;
    bool json_;
    std::vector<std::size_t> offsets_, lengths_;
    std::vector<std::int32_t> ids_;
    std::vector<Json> keys_;
    std::unordered_map<std::int32_t,std::size_t> references_;
    std::shared_ptr<LoadBudget> budget_;
    std::shared_ptr<const std::unordered_map<std::string,IndexedRows*>> references_directory_;
    IndexedRows(const std::string& schema,std::vector<std::uint8_t> bytes,bool json,std::shared_ptr<LoadBudget> budget):wire_(schema),json_bounds_(wire_.schema_,budget->limits()),json_(json),budget_(std::move(budget)) {
        DecodeScope scope(budget_->limits());const auto& key_fields=wire_.schema_.field("keys").array();
        auto key_from=[&](const Json& row){Json key(Json::Kind::Object);for(const auto& name:key_fields)key.members.emplace(name.string(),row.field(name.string()));return key;};
        if(json){
            auto doc=JsonParser(std::string(bytes.begin(),bytes.end()),json_bounds_).parse();
            if(doc.members.size()!=4||doc.field("format").string()!="ZrConfig.TableJson/1"||doc.field("table").string()!=wire_.name()||doc.field("schemaHash").string()!=wire_.hash())throw ConfigError(ErrorCode::SchemaMismatch,"JSON header mismatch");
            const auto& rows=doc.field("rows").array();if(rows.size()>max_rows)throw ConfigError(ErrorCode::LimitExceeded,"Too many rows");
            for(const auto& row:rows){keys_.push_back(key_from(row));ids_.push_back(wire_.schema_.field("hasId").boolean()?integer<std::int32_t>(row.field("Id")):0);std::string fragment;append_json(fragment,row);fragments_.push_back(std::move(fragment));}
        }else{
            bytes_=std::move(bytes);Cursor cursor{bytes_};
            if(cursor.i32()!=0x4643525a||cursor.i32()!=4||cursor.text()!=wire_.name()||cursor.text()!=wire_.hash())throw ConfigError(ErrorCode::SchemaMismatch,"Binary header mismatch");
            auto count=cursor.count(max_rows);if(count>(bytes_.size()-cursor.pos)/12)throw ConfigError(ErrorCode::InvalidValue,"Truncated row index");
            auto nt=cursor.count(wire_.schema_.field("tags").array().size());std::unordered_set<std::string> seen;
            for(std::size_t i=0;i<nt;++i){auto tag=cursor.text();if(!Wire::contains(wire_.schema_.field("tags"),tag)||!seen.insert(tag).second)throw ConfigError(ErrorCode::InvalidValue,"Invalid type directory");wire_.tags_.push_back(std::move(tag));}
            std::size_t end=0;
            for(std::size_t i=0;i<count;++i){
                wire_.remaining_=detail::decode_elements();Json key(Json::Kind::Object);for(const auto& name:key_fields)for(const auto& field:wire_.schema_.field("fields").array())if(field.field("name").string()==name.string())key.members.emplace(name.string(),wire_.value(field.field("type").string(),cursor,0));
                keys_.push_back(std::move(key));ids_.push_back(cursor.i32());auto offset=cursor.i32(),length=cursor.i32();
                if(offset<0||length<0||static_cast<std::size_t>(offset)!=end||static_cast<std::size_t>(length)>bytes_.size()-end)throw ConfigError(ErrorCode::InvalidValue,"Invalid row bounds");
                offsets_.push_back(static_cast<std::size_t>(offset));lengths_.push_back(static_cast<std::size_t>(length));end+=static_cast<std::size_t>(length);
            }
            if(cursor.pos+end!=bytes_.size())throw ConfigError(ErrorCode::InvalidValue,"Invalid payload size");
            for(auto& offset:offsets_)offset+=cursor.pos;
        }
        if(wire_.schema_.field("hasId").boolean())for(std::size_t i=0;i<ids_.size();++i){auto added=references_.emplace(ids_[i],i);if(!added.second)added.first->second=(std::numeric_limits<std::size_t>::max)();}
    }
public:
    static std::unique_ptr<IndexedRows> open(SourceBundle& bundle,const std::string& schema,bool prefer_json){
        Wire wire(schema);const auto& entry=bundle.entry(wire.name());if(entry.field("schemaHash").string()!=wire.hash())throw ConfigError(ErrorCode::SchemaMismatch,"Schema mismatch");
        bundle.budget()->rows(integer<std::uint32_t>(entry.field("rows")));bool alternate=prefer_json&&entry.members.count("jsonFile")&&entry.field("jsonFile").kind==Json::Kind::String;
        auto file=path_segment(entry.field(alternate?"jsonFile":"file").string());auto bytes=bundle.read(file);auto expected=entry.field(alternate?"jsonSha256":"sha256").string();
        std::transform(expected.begin(),expected.end(),expected.begin(),[](unsigned char c){return static_cast<char>(std::tolower(c));});
        if(sha256(bytes)!=expected)throw ConfigError(ErrorCode::IntegrityMismatch,"Table digest mismatch");
        auto result=std::unique_ptr<IndexedRows>(new IndexedRows(schema,std::move(bytes),file.size()>=5&&file.substr(file.size()-5)==".json",bundle.budget()));
        result->version_=bundle.version();result->relative_path_="versions/"+bundle.version()+"/"+file;
        if(result->count()!=integer<std::uint32_t>(entry.field("rows")))throw ConfigError(ErrorCode::InvalidValue,"Row count mismatch");
        std::size_t raw=result->bytes_.size();
        for(const auto& part:result->fragments_)raw+=part.size();
        result->index_raw_bytes_=raw;bundle.budget()->raw(raw);return result;
    }
    void bind_references(std::shared_ptr<const std::unordered_map<std::string,IndexedRows*>> directory){references_directory_=std::move(directory);}
    std::size_t count()const noexcept{return ids_.size();}
    const Json& key(std::size_t index)const{return keys_.at(index);}
    const Json& schema()const noexcept{return wire_.schema_;}
    std::size_t reference(std::int32_t id)const{auto found=references_.find(id);if(found==references_.end()||found->second==(std::numeric_limits<std::size_t>::max)())throw ConfigError(ErrorCode::MissingReference,"Missing or ambiguous reference");return found->second;}
    LoadBudget& budget()const noexcept{return *budget_;}
    std::uint64_t estimated_row_bytes(std::size_t index)const{return static_cast<std::uint64_t>(json_?fragments_.at(index).size():lengths_.at(index))*4+128;}
    std::uint64_t estimated_index_bytes(std::size_t fields)const{return 256+512*static_cast<std::uint64_t>(count())*(std::max)(std::size_t{1},fields)+8*index_raw_bytes_;}
    Json read_transient(std::size_t index){LoadBudget::Decode permit(*budget_,estimated_row_bytes(index),true);return read(index);}
    Json read(std::size_t index){
        try{return read_value(index);}catch(const ConfigError& error){std::string key;append_json(key,keys_.at(index));throw error.locate("row",{version_,wire_.name(),key,"",relative_path_});}catch(const InvalidData& error){std::string key;append_json(key,keys_.at(index));throw ConfigError(ErrorCode::InvalidValue,error.what(),"row",0,{version_,wire_.name(),key,"",relative_path_},std::current_exception());}
    }
    Json read_value(std::size_t index){
        DecodeScope scope(budget_->limits());wire_.remaining_=detail::decode_elements();Json row;
        if(json_)row=JsonParser(fragments_.at(index),json_bounds_).parse();else{
            // Cursor's existing public aggregate layout stays unchanged. The temporary
            // bounded row buffer prevents a malformed decoder from reading a neighbour.
            std::vector<std::uint8_t> bytes(bytes_.begin()+offsets_.at(index),bytes_.begin()+offsets_[index]+lengths_[index]);Cursor cursor{bytes};
            row=wire_.fields(wire_.schema_.field("fields"),cursor,0);if(cursor.pos!=bytes.size())throw ConfigError(ErrorCode::InvalidValue,"Row boundary mismatch");
        }
        wire_.remaining_=detail::decode_elements();wire_.validate_fields(wire_.schema_.field("fields"),row,0,false);
        for(const auto& key:keys_.at(index).members){const auto& value=row.field(key.first);if(value.kind!=key.second.kind||value.text!=key.second.text)throw ConfigError(ErrorCode::InvalidValue,"Row key differs from index");}
        if(ids_[index]!=(wire_.schema_.field("hasId").boolean()?integer<std::int32_t>(row.field("Id")):0))throw ConfigError(ErrorCode::InvalidValue,"Row Id differs from index");
        if(references_directory_)validate_references(row,[&](const std::string& name)->IndexedRows&{return *references_directory_->at(name);});
        return row;
    }
    template<class Resolve>void validate_references(const Json& row,Resolve resolve)const{
        for(const auto& field:wire_.schema_.field("fields").array()){const auto& type=*wire_.types_.at(field.field("type").string());if(type.field("kind").string()=="tablereference"){auto id=integer<std::int32_t>(row.field(field.field("name").string()));if(id)try{resolve(type.field("referenceTable").string()).reference(id);}catch(const ConfigError& error){throw error.locate("reference",{version_,wire_.name(),"",field.field("name").string(),relative_path_});}}}
    }
};
template<class T>class DerivedValue {
    std::unique_ptr<T> owned_;std::atomic<T*> ready_{nullptr};mutable std::mutex gate_;std::condition_variable done_;bool running_=false;std::exception_ptr failure_;
public:
    T& get()const{if(auto value=ready_.load(std::memory_order_acquire))return *value;std::lock_guard<std::mutex> lock(gate_);if(failure_)std::rethrow_exception(failure_);throw ConfigError(ErrorCode::TableNotLoaded,"Load the group index explicitly");}
    template<class Factory>T& load(const Cancellation& token,LoadBudget& budget,std::uint64_t estimate,Factory factory){
        token.check();if(auto value=ready_.load(std::memory_order_acquire))return *value;
        std::unique_lock<std::mutex> lock(gate_);std::optional<LoadBudget::Request> waiting;
        while(true){token.check();if(auto value=ready_.load(std::memory_order_acquire))return *value;if(failure_)std::rethrow_exception(failure_);if(!running_)break;if(!waiting)waiting.emplace(budget,false);done_.wait_for(lock,std::chrono::milliseconds(10));}
        waiting.reset();LoadBudget::Request permit(budget,true);running_=true;lock.unlock();std::exception_ptr error;bool terminal=false;
        try{LoadBudget::Index index_permit(budget,estimate);auto value=factory();index_permit.commit(value.second);owned_=std::move(value.first);ready_.store(owned_.get(),std::memory_order_release);}
        catch(const ConfigError& e){terminal=e.code==ErrorCode::InvalidValue||e.code==ErrorCode::MissingReference||e.code==ErrorCode::IntegrityMismatch||e.code==ErrorCode::SchemaMismatch||e.code==ErrorCode::InvalidEncoding;error=std::current_exception();}
        catch(const InvalidData& e){try{error=std::make_exception_ptr(ConfigError(ErrorCode::InvalidValue,e.what(),"group"));terminal=true;}catch(...){error=std::current_exception();}}
        catch(...){error=std::current_exception();}
        lock.lock();if(terminal){failure_=error;budget.index_fault();}running_=false;lock.unlock();done_.notify_all();token.check();if(error)std::rethrow_exception(error);return get();
    }
};
// Share scheduling and ownership machinery across row types. A storage instance
// fixes its decoder/deleter pair at construction; typed wrappers never mix rows.
// Cached reads retain the same atomic pointer load and do not allocate or cast
// through RTTI. All decoded rows belong to this storage until snapshot destruction.
class ErasedRowStorage {
    struct Work{std::mutex gate;std::condition_variable done;bool complete=false;std::exception_ptr error;const void* value=nullptr;};
    const void*(*decode_)(const Json&);void(*destroy_)(const void*);
    std::unique_ptr<IndexedRows> indexed_;
    std::vector<const void*> values_;
    std::unique_ptr<std::atomic<const void*>[]> ready_;
    mutable std::mutex gate_;
    std::unordered_map<std::size_t,std::shared_ptr<Work>> pending_;
    std::vector<std::exception_ptr> failures_;
    std::atomic<std::size_t> loaded_{0};
public:
    explicit ErasedRowStorage(std::unique_ptr<IndexedRows> indexed,const void*(*decode)(const Json&),void(*destroy)(const void*)):decode_(decode),destroy_(destroy),indexed_(std::move(indexed)),values_(indexed_->count()),ready_(std::make_unique<std::atomic<const void*>[]>(indexed_->count())),failures_(indexed_->count()){for(std::size_t i=0;i<count();++i)ready_[i].store(nullptr,std::memory_order_relaxed);}
    ~ErasedRowStorage(){for(auto value:values_)if(value)destroy_(value);}
    IndexedRows& indexed()const noexcept{return *indexed_;}
    std::size_t count()const noexcept{return values_.size();}
    std::size_t loaded_count()const noexcept{return loaded_.load(std::memory_order_relaxed);}
    const void* cached(std::size_t index)const noexcept{return ready_[index].load(std::memory_order_acquire);}
    const void* get(std::size_t index)const{
        if(index>=count())throw ConfigError(ErrorCode::KeyNotFound,"Row index not found");
        if(auto row=ready_[index].load(std::memory_order_acquire))return row;
        std::lock_guard<std::mutex> lock(gate_);if(failures_[index])std::rethrow_exception(failures_[index]);throw ConfigError(ErrorCode::RowNotLoaded,"Load the row explicitly");
    }
    const void* load(std::size_t index,const Cancellation& cancellation){
        cancellation.check();if(index>=count())throw ConfigError(ErrorCode::KeyNotFound,"Row index not found");if(auto row=ready_[index].load(std::memory_order_acquire))return row;
        std::shared_ptr<Work> work;bool owner=false;std::optional<LoadBudget::Request> request;
        {std::lock_guard<std::mutex> lock(gate_);if(auto row=ready_[index].load(std::memory_order_acquire))return row;if(failures_[index])std::rethrow_exception(failures_[index]);auto found=pending_.find(index);if(found!=pending_.end()){request.emplace(indexed_->budget(),false);work=found->second;}else{request.emplace(indexed_->budget(),true);work=std::make_shared<Work>();pending_.emplace(index,work);owner=true;}}
        if(owner){
            bool terminal=false;std::exception_ptr error;const void* row=nullptr;
            try{LoadBudget::Decode permit(indexed_->budget(),indexed_->estimated_row_bytes(index));row=decode_(indexed_->read(index));values_[index]=row;ready_[index].store(row,std::memory_order_release);loaded_.fetch_add(1,std::memory_order_relaxed);permit.commit();}
            catch(const ConfigError& failure){terminal=failure.code==ErrorCode::InvalidValue||failure.code==ErrorCode::MissingReference||failure.code==ErrorCode::IntegrityMismatch||failure.code==ErrorCode::SchemaMismatch||failure.code==ErrorCode::InvalidEncoding;error=std::current_exception();}
            catch(const InvalidData& value){try{error=std::make_exception_ptr(ConfigError(ErrorCode::InvalidValue,value.what(),"row"));terminal=true;}catch(...){error=std::current_exception();}}
            catch(...){error=std::current_exception();}
            {std::lock_guard<std::mutex> lock(gate_);if(terminal){failures_[index]=error;indexed_->budget().fault();}pending_.erase(index);}
            {std::lock_guard<std::mutex> lock(work->gate);work->value=row;work->error=error;work->complete=true;}work->done.notify_all();
        }
        std::unique_lock<std::mutex> lock(work->gate);while(!work->complete){cancellation.check();work->done.wait_for(lock,std::chrono::milliseconds(10));}
        cancellation.check();if(work->error)std::rethrow_exception(work->error);return work->value;
    }
};
template<class T>class RowStorage:public ErasedRowStorage {
public:
    explicit RowStorage(std::unique_ptr<IndexedRows> indexed):ErasedRowStorage(std::move(indexed),+[](const Json& value)->const void*{return new const T(value);},+[](const void* value){delete static_cast<const T*>(value);}){}
    const T* cached(std::size_t index)const noexcept{return static_cast<const T*>(ErasedRowStorage::cached(index));}
    const T* get(std::size_t index)const{return static_cast<const T*>(ErasedRowStorage::get(index));}
    const T* load(std::size_t index,const Cancellation& cancellation){return static_cast<const T*>(ErasedRowStorage::load(index,cancellation));}
};
}
struct PreparationOptions {
    std::chrono::milliseconds timeout{300000};
    std::chrono::milliseconds candidate_lifetime{300000};
};
enum class ManagerPhase { Empty, Ready, Closed };
struct ManagerStatus {
    ManagerPhase phase;
    std::uint64_t generation;
    std::uint64_t preparing_operation;
};

// All owning projections alias the SAME snapshot control block. Storage never owns a projection.
// borrow() is explicitly scoped: retaining its return value alone does not retain the snapshot.
class ConfigSnapshot;
namespace server { class Snapshot; }
namespace data { class Snapshot; }
template<class S,class T> class DataRowHandle;
template<class T> class OwnedValue {
    friend class server::Snapshot;
    template<class> friend class OwnedValue;
    template<class> friend class OwnedGroup;
    template<class,class> friend class DataRowHandle;
    std::shared_ptr<const T> value_;
    template<class Owner> OwnedValue(const std::shared_ptr<const Owner>& owner, const T& value)
        : value_(owner, &value) { if (!owner) throw ConfigError(ErrorCode::InvalidPlan, "Missing owner"); }
public:
    const T& borrow() const & noexcept { return *value_; }
    const T& borrow() const && = delete;
    template<class V, class U = T>
    OwnedValue<V> select(const V& (U::*member)() const noexcept) const {
        return OwnedValue<V>(value_, ((*value_).*member)());
    }
    template<class V, class U = T>
    std::optional<OwnedValue<V>> reference(const V* (U::*member)() const noexcept) const {
        const auto* value = ((*value_).*member)();
        if (!value) return std::nullopt;
        return OwnedValue<V>(value_, *value);
    }
};
template<class T> class OwnedGroup {
    friend class server::Snapshot;
    OwnedValue<std::vector<const T*>> values_;
    template<class Owner> OwnedGroup(const std::shared_ptr<const Owner>& owner, const std::vector<const T*>& values)
        : values_(owner, values), owner_(owner) {}
public:
    std::size_t size() const noexcept { return values_.borrow().size(); }
    OwnedValue<T> at(std::size_t index) const { return OwnedValue<T>(owner_, *values_.borrow().at(index)); }
private:
    std::shared_ptr<const void> owner_;
};

template<class S,class T>class DataRowHandle {
protected:
    std::shared_ptr<const S> owner_;
    const T* value_;
    DataRowHandle(std::shared_ptr<const S> owner,const T* value):owner_(std::move(owner)),value_(value){if(!owner_||!value_)throw ConfigError(ErrorCode::InvalidPlan,"Invalid row owner");}
public:
    const T& borrow()const & noexcept{return *value_;}
    const T& borrow()const && = delete;
    template<class V>OwnedValue<V> select(const V&(T::*member)()const noexcept)const{return OwnedValue<V>(owner_,(value_->*member)());}
};
template<class S,class R>class DataGroup {
    friend class data::Snapshot;
    std::shared_ptr<const S> owner_;
    const std::vector<std::size_t>* ordinals_;
    R(*query_)(const std::shared_ptr<const S>&,std::size_t);
    DataGroup(std::shared_ptr<const S> owner,const std::vector<std::size_t>& ordinals,R(*query)(const std::shared_ptr<const S>&,std::size_t)):owner_(std::move(owner)),ordinals_(&ordinals),query_(query){}
public:
    std::size_t size()const noexcept{return ordinals_->size();}
    R at(std::size_t index)const{return query_(owner_,ordinals_->at(index));}
};

class SnapshotIdentity {
    std::string namespace_,version_,digest_;
public:
    SnapshotIdentity(std::string source_namespace,std::string version_id,std::string manifest_digest):namespace_(std::move(source_namespace)),version_(std::move(version_id)),digest_(std::move(manifest_digest)){if(version_.empty()||digest_.empty())throw ConfigError(ErrorCode::InvalidPlan,"Incomplete snapshot identity");}
    const std::string& source_namespace()const noexcept{return namespace_;}
    const std::string& version_id()const noexcept{return version_;}
    const std::string& manifest_digest()const noexcept{return digest_;}
};
enum class ManagerEventKind{Committed,Cleared,Closed,PreparationFailed};
struct ManagerEvent{ManagerEventKind kind;std::uint64_t generation=0,operation_id=0;std::optional<SnapshotIdentity> identity;std::optional<ErrorCode> error=std::nullopt;};
struct ManagerDiagnostics{std::size_t pending_events,active_preparations,retained_identities,subscribers;std::uint64_t dropped_events,callback_failures;};
template<class S>class ManagerPolicy {
    template<class>friend class ServerManager;
    std::function<SnapshotIdentity(const S&)> identity_;
    std::vector<std::function<void(const S&,const Cancellation&)>> validators_;
    std::size_t event_capacity_=64,identity_capacity_=32,preparation_capacity_=64;
    std::uint64_t max_live_bytes_=(std::numeric_limits<std::uint64_t>::max)();
    std::size_t live_capacity_=1024;
    std::function<std::uint64_t(const S&)> estimate_;
    static void positive(std::size_t value){if(!value)throw ConfigError(ErrorCode::InvalidPlan,"Policy capacities must be positive");}
public:
    ManagerPolicy& with_identity(std::function<SnapshotIdentity(const S&)> resolver){if(!resolver)throw ConfigError(ErrorCode::InvalidPlan,"Missing identity resolver");identity_=std::move(resolver);return *this;}
    ManagerPolicy& with_validator(std::function<void(const S&,const Cancellation&)> validator){if(!validator)throw ConfigError(ErrorCode::InvalidPlan,"Missing validator");validators_.push_back(std::move(validator));return *this;}
    ManagerPolicy& with_event_capacity(std::size_t value){positive(value);event_capacity_=value;return *this;}
    ManagerPolicy& with_identity_capacity(std::size_t value){positive(value);identity_capacity_=value;return *this;}
    ManagerPolicy& with_preparation_capacity(std::size_t value){positive(value);preparation_capacity_=value;return *this;}
    ManagerPolicy& with_estimated_live_budget(std::uint64_t maximum,std::function<std::uint64_t(const S&)> estimate={},std::size_t tracking_capacity=1024){if(!maximum||!tracking_capacity)throw ConfigError(ErrorCode::InvalidPlan,"Invalid live memory budget");max_live_bytes_=maximum;estimate_=std::move(estimate);live_capacity_=tracking_capacity;return *this;}
};
namespace detail {
template<class S,class=void>struct HasSnapshotIdentity:std::false_type{};
template<class S>struct HasSnapshotIdentity<S,std::void_t<decltype(std::declval<const S&>().source_namespace()),decltype(std::declval<const S&>().version()),decltype(std::declval<const S&>().manifest_digest())>>:std::true_type{};
template<class S,class=void>struct HasSnapshotDiagnostics:std::false_type{};
template<class D,class=void>struct IndexEstimate{static std::uint64_t get(const D&){return 0;}};
template<class D>struct IndexEstimate<D,std::void_t<decltype(std::declval<const D&>().estimated_index_bytes)>>{static std::uint64_t get(const D& d){return d.estimated_index_bytes;}};
template<class S>struct HasSnapshotDiagnostics<S,std::void_t<decltype(std::declval<const S&>().diagnostics().retained_raw_bytes),decltype(std::declval<const S&>().diagnostics().estimated_decoded_bytes),decltype(std::declval<const S&>().diagnostics().declared_rows)>>:std::true_type{};
}
// Elapsed monotonic milliseconds include waits and exclude callbacks. Operation zero means no sample.
struct StageTiming{const char* stage;std::uint64_t operation_id;double elapsed_milliseconds;bool succeeded;};
template<class Snapshot>class ServerManager {
    using Clock=std::chrono::steady_clock;
    using Listener=std::function<void(const ManagerEvent&)>;
    struct State {
        std::mutex gate;std::shared_ptr<const Snapshot> current;std::atomic<bool> closed{false};std::uint64_t generation=0,serial=0,pending=0;bool building=false;Clock::time_point expires{};
        std::optional<Cancellation> cancellation;std::size_t active=0;std::vector<SnapshotIdentity> identities;
        std::vector<std::weak_ptr<const Snapshot>> live;
        std::array<StageTiming,2> timings{{{"Prepare",0,0,false},{"Commit",0,0,false}}};
    };
    std::shared_ptr<State> state_=std::make_shared<State>();
    const ManagerPolicy<Snapshot> policy_;
    mutable std::mutex event_gate_;std::deque<ManagerEvent> events_;std::map<std::uint64_t,std::shared_ptr<const Listener>> listeners_;std::uint64_t listener_serial_=0,dropped_=0,callback_failures_=0;bool draining_=false;
    static void advance(State& state){if(state.generation==(std::numeric_limits<std::uint64_t>::max)())throw ConfigError(ErrorCode::LimitExceeded,"Generation exhausted");++state.generation;}
    static Clock::time_point deadline(std::chrono::milliseconds duration){auto now=Clock::now();if(duration.count()<=0||duration>std::chrono::duration_cast<std::chrono::milliseconds>((Clock::time_point::max)()-now))throw ConfigError(ErrorCode::InvalidPlan,"Invalid preparation deadline");return now+duration;}
    static void check_preparation(const State& state,std::uint64_t generation,std::uint64_t operation){if(state.closed)throw ConfigError(ErrorCode::ManagerClosed,"Manager closed");if(state.generation!=generation||state.pending!=operation)throw ConfigError(ErrorCode::StaleCandidate,"Preparation became stale");}
    static void check_identity(const State& state,const std::optional<SnapshotIdentity>& identity){if(identity)for(const auto& old:state.identities)if(old.source_namespace()==identity->source_namespace()&&old.version_id()==identity->version_id()&&old.manifest_digest()!=identity->manifest_digest())throw ConfigError(ErrorCode::VersionIdentityConflict,"Version reused with different manifest bytes");}
    std::optional<SnapshotIdentity> identity(const Snapshot& snapshot)const{if(policy_.identity_)return policy_.identity_(snapshot);if constexpr(detail::HasSnapshotIdentity<Snapshot>::value)return SnapshotIdentity(snapshot.source_namespace(),snapshot.version(),snapshot.manifest_digest());else return std::nullopt;}
    bool limits_live()const noexcept{return policy_.max_live_bytes_!=(std::numeric_limits<std::uint64_t>::max)();}
    std::uint64_t estimate(const Snapshot& snapshot)const{
        if(policy_.estimate_)return policy_.estimate_(snapshot);
        if constexpr(detail::HasSnapshotDiagnostics<Snapshot>::value){auto value=snapshot.diagnostics();auto maximum=(std::numeric_limits<std::uint64_t>::max)();if(value.declared_rows>maximum/64)return maximum;auto bytes=static_cast<std::uint64_t>(value.declared_rows)*64;if(value.retained_raw_bytes>maximum-bytes)return maximum;bytes+=value.retained_raw_bytes;auto indexes=detail::IndexEstimate<decltype(value)>::get(value);if(indexes>maximum-bytes)return maximum;bytes+=indexes;return value.estimated_decoded_bytes>maximum-bytes?maximum:bytes+value.estimated_decoded_bytes;}
        else throw ConfigError(ErrorCode::UnsupportedCapability,"A live memory budget requires a snapshot estimate provider");
    }
    std::uint64_t live_usage(const std::shared_ptr<const Snapshot>& candidate={},bool enforce=true)const{
        // Weak tracking never keeps retired snapshots alive. All user estimators and
        // last-owner destruction run outside the manager gate.
        std::vector<std::weak_ptr<const Snapshot>> weak;
        {std::lock_guard<std::mutex> lock(state_->gate);auto& live=state_->live;live.erase(std::remove_if(live.begin(),live.end(),[](const auto& value){return value.expired();}),live.end());weak=live;}
        std::vector<std::shared_ptr<const Snapshot>> owners;owners.reserve(weak.size()+1);bool included=false;
        for(const auto& reference:weak)if(auto owner=reference.lock()){included|=owner==candidate;owners.push_back(std::move(owner));}
        if(candidate&&!included)owners.push_back(candidate);
        std::uint64_t total=0;const auto maximum=(std::numeric_limits<std::uint64_t>::max)();for(const auto& owner:owners){auto bytes=estimate(*owner);total=bytes>maximum-total?maximum:total+bytes;if(enforce&&total>policy_.max_live_bytes_)throw ConfigError(ErrorCode::BudgetExceeded,"Estimated live snapshot budget exhausted","admission");}return total;
    }
    void track_live(State& state,const std::shared_ptr<const Snapshot>& snapshot){
        if(!limits_live())return;
        state.live.erase(std::remove_if(state.live.begin(),state.live.end(),[](const auto& value){return value.expired();}),state.live.end());
        for(const auto& value:state.live)if(!value.owner_before(snapshot)&&!snapshot.owner_before(value)&&value.lock().get()==snapshot.get())return;
        if(state.live.size()>=policy_.live_capacity_)throw ConfigError(ErrorCode::BudgetExceeded,"Live snapshot tracking capacity exhausted","admission");
        state.live.emplace_back(snapshot);
    }
    // Publication holds the state gate. Notification failure can never roll back publication.
    void enqueue(ManagerEvent event)noexcept{std::lock_guard<std::mutex> lock(event_gate_);if(listeners_.empty())return;try{if(events_.size()>=policy_.event_capacity_){events_.pop_front();++dropped_;}events_.push_back(std::move(event));}catch(...){++dropped_;}}
    void drain()noexcept{
        {std::lock_guard<std::mutex> lock(event_gate_);if(draining_)return;draining_=true;}
        while(true){std::optional<ManagerEvent> event;std::vector<std::shared_ptr<const Listener>> callbacks;
            {std::lock_guard<std::mutex> lock(event_gate_);if(events_.empty()){draining_=false;return;}event.emplace(std::move(events_.front()));events_.pop_front();try{callbacks.reserve(listeners_.size());for(const auto& listener:listeners_)callbacks.push_back(listener.second);}catch(...){++dropped_;continue;}}
            for(const auto& callback:callbacks)try{(*callback)(*event);}catch(...){std::lock_guard<std::mutex> lock(event_gate_);++callback_failures_;}
        }
    }
    struct ActivePreparation{std::shared_ptr<State> state;bool active=true;void finish(){if(active){std::lock_guard<std::mutex> lock(state->gate);--state->active;active=false;}}~ActivePreparation(){finish();}};
public:
    class Prepared {
        friend class ServerManager;std::weak_ptr<State> owner_;std::shared_ptr<const Snapshot> snapshot_;std::uint64_t generation_,operation_;Clock::time_point expires_;bool committed_=false;std::optional<SnapshotIdentity> identity_;
        Prepared(const std::shared_ptr<State>& owner,std::shared_ptr<const Snapshot> value,std::uint64_t generation,std::uint64_t operation,Clock::time_point expires,std::optional<SnapshotIdentity> identity):owner_(owner),snapshot_(std::move(value)),generation_(generation),operation_(operation),expires_(expires),identity_(std::move(identity)){}
    public:
        Prepared(const Prepared&)=delete;Prepared& operator=(const Prepared&)=delete;Prepared(Prepared&&)noexcept=default;Prepared& operator=(Prepared&&)=delete;
        ~Prepared(){discard();}
        std::shared_ptr<const Snapshot> snapshot()const{return snapshot_;}
        void discard()noexcept{if(auto state=owner_.lock()){std::lock_guard<std::mutex> lock(state->gate);if(!committed_&&state->pending==operation_){state->pending=0;if(state->cancellation){state->cancellation->cancel();state->cancellation.reset();}}}snapshot_.reset();}
    };
    struct CommitResult{std::shared_ptr<const Snapshot> snapshot;std::uint64_t generation,operation_id;bool committed=true;};
    ServerManager():ServerManager(ManagerPolicy<Snapshot>{}){}
    explicit ServerManager(ManagerPolicy<Snapshot> policy):policy_(std::move(policy)){state_->identities.reserve(policy_.identity_capacity_);if(limits_live()&&!policy_.estimate_&&!detail::HasSnapshotDiagnostics<Snapshot>::value)throw ConfigError(ErrorCode::UnsupportedCapability,"A live memory budget requires a snapshot estimate provider");}
    ServerManager(const ServerManager&)=delete;ServerManager& operator=(const ServerManager&)=delete;
    std::shared_ptr<const Snapshot> try_acquire_snapshot()const{auto state=state_;auto result=std::atomic_load_explicit(&state->current,std::memory_order_acquire);if(state->closed.load(std::memory_order_acquire))throw ConfigError(ErrorCode::ManagerClosed,"Manager closed");return result;}
    std::shared_ptr<const Snapshot> acquire_snapshot()const{auto result=try_acquire_snapshot();if(!result)throw ConfigError(ErrorCode::NotInitialized,"Configuration not loaded");return result;}
    ManagerStatus status()const{auto state=state_;std::lock_guard<std::mutex> lock(state->gate);return {state->closed?ManagerPhase::Closed:(std::atomic_load(&state->current)?ManagerPhase::Ready:ManagerPhase::Empty),state->generation,state->pending};}
    std::array<StageTiming,2> stage_timings()const{auto state=state_;std::lock_guard<std::mutex> lock(state->gate);return state->timings;}
    ManagerDiagnostics diagnostics()const{auto state=state_;std::lock_guard<std::mutex> lock(state->gate);std::lock_guard<std::mutex> events(event_gate_);return {events_.size(),state->active,state->identities.size(),listeners_.size(),dropped_,callback_failures_};}
    std::optional<std::uint64_t> estimated_live_bytes()const{if(!limits_live())return std::nullopt;return live_usage({},false);}
    std::uint64_t subscribe(Listener callback){if(!callback)throw ConfigError(ErrorCode::InvalidPlan,"Missing event listener");auto listener=std::make_shared<const Listener>(std::move(callback));std::lock_guard<std::mutex> lock(event_gate_);if(listener_serial_==(std::numeric_limits<std::uint64_t>::max)())throw ConfigError(ErrorCode::LimitExceeded,"Listener identifiers exhausted");auto id=++listener_serial_;listeners_.emplace(id,std::move(listener));return id;}
    bool unsubscribe(std::uint64_t id){std::shared_ptr<const Listener> removed;{std::lock_guard<std::mutex> lock(event_gate_);auto found=listeners_.find(id);if(found==listeners_.end())return false;removed=std::move(found->second);listeners_.erase(found);}return true;}
    template<class Factory>Prepared prepare(Factory&& factory,const Cancellation& cancellation,PreparationOptions options={}){return prepare_with_cancellation([&](const Cancellation&){return factory();},cancellation,options);}
    template<class Factory>Prepared prepare_with_cancellation(Factory&& factory,const Cancellation& cancellation,PreparationOptions options={}){
        const auto started=Clock::now();cancellation.check();const auto expires_at=deadline(options.timeout);deadline(options.candidate_lifetime);auto operation_token=cancellation.child(expires_at);auto state=state_;std::uint64_t generation,operation;
        {std::lock_guard<std::mutex> lock(state->gate);if(state->closed)throw ConfigError(ErrorCode::ManagerClosed,"Manager closed");if(state->pending&&(state->building||Clock::now()<state->expires))throw ConfigError(ErrorCode::ReloadBusy,"Preparation already in progress");if(state->active>=policy_.preparation_capacity_)throw ConfigError(ErrorCode::LimitExceeded,"Preparation capacity exhausted");if(state->serial==(std::numeric_limits<std::uint64_t>::max)())throw ConfigError(ErrorCode::LimitExceeded,"Operation identifiers exhausted");operation=++state->serial;generation=state->generation;state->pending=operation;state->expires=expires_at;state->building=true;++state->active;state->cancellation=operation_token;}
        ActivePreparation active{state};
        try{
            if(limits_live())live_usage();
            std::shared_ptr<const Snapshot> next=factory(operation_token);{std::lock_guard<std::mutex> lock(state->gate);check_preparation(*state,generation,operation);}operation_token.check();if(!next)throw ConfigError(ErrorCode::InvalidValue,"Factory returned no snapshot","prepare",operation);
            for(const auto& validator:policy_.validators_){validator(*next,operation_token);{std::lock_guard<std::mutex> lock(state->gate);check_preparation(*state,generation,operation);}operation_token.check();}
            auto metadata=identity(*next);operation_token.check();auto expires=deadline(options.candidate_lifetime);
            if(limits_live())live_usage(next);
            {std::lock_guard<std::mutex> lock(state->gate);check_preparation(*state,generation,operation);check_identity(*state,metadata);track_live(*state,next);state->expires=expires;state->building=false;state->cancellation.reset();if(operation>=state->timings[0].operation_id)state->timings[0]={"Prepare",operation,std::chrono::duration<double,std::milli>(Clock::now()-started).count(),true};}
            return Prepared(state,std::move(next),generation,operation,expires,std::move(metadata));
        }catch(...){
            auto code=ErrorCode::InternalError;try{throw;}catch(const ConfigError& error){code=error.code;}catch(...){}
            active.finish();{std::lock_guard<std::mutex> lock(state->gate);if(operation>=state->timings[0].operation_id)state->timings[0]={"Prepare",operation,std::chrono::duration<double,std::milli>(Clock::now()-started).count(),false};if(state->pending==operation){state->pending=0;state->building=false;state->cancellation.reset();}enqueue(ManagerEvent{ManagerEventKind::PreparationFailed,generation,operation,std::nullopt,code});}
            drain();throw;
        }
    }
    CommitResult commit(Prepared& prepared,const Cancellation& cancellation){
        const auto started=Clock::now();
        auto state=state_;if(prepared.owner_.lock()!=state)throw ConfigError(ErrorCode::ForeignCandidate,"Candidate belongs to another manager");std::shared_ptr<const Snapshot> previous;std::uint64_t generation;
        {std::lock_guard<std::mutex> lock(state->gate);if(state->closed)throw ConfigError(ErrorCode::ManagerClosed,"Manager closed");if(prepared.committed_)throw ConfigError(ErrorCode::AlreadyCommitted,"Candidate already committed");check_preparation(*state,prepared.generation_,prepared.operation_);if(!prepared.snapshot_)throw ConfigError(ErrorCode::StaleCandidate,"Candidate discarded");cancellation.check();}
        ManagerEvent event{ManagerEventKind::Committed,0,prepared.operation_,prepared.identity_};auto accepted=prepared.identity_; // Allocate copies before publication.
        if(limits_live())live_usage(prepared.snapshot_);
        {std::lock_guard<std::mutex> lock(state->gate);if(state->closed)throw ConfigError(ErrorCode::ManagerClosed,"Manager closed");if(prepared.committed_)throw ConfigError(ErrorCode::AlreadyCommitted,"Candidate already committed");check_preparation(*state,prepared.generation_,prepared.operation_);if(!prepared.snapshot_)throw ConfigError(ErrorCode::StaleCandidate,"Candidate discarded");cancellation.check();if(Clock::now()>=prepared.expires_)throw ConfigError(ErrorCode::DeadlineExceeded,"Candidate expired");check_identity(*state,accepted);
            advance(*state);generation=state->generation;
            if(accepted){auto found=std::find_if(state->identities.begin(),state->identities.end(),[&](const SnapshotIdentity& old){return old.source_namespace()==accepted->source_namespace()&&old.version_id()==accepted->version_id();});if(found!=state->identities.end())state->identities.erase(found);if(state->identities.size()>=policy_.identity_capacity_)state->identities.erase(state->identities.begin());state->identities.push_back(std::move(*accepted));}
            previous=std::atomic_exchange_explicit(&state->current,prepared.snapshot_,std::memory_order_acq_rel);prepared.committed_=true;state->pending=0;event.generation=generation;state->timings[1]={"Commit",prepared.operation_,std::chrono::duration<double,std::milli>(Clock::now()-started).count(),true};enqueue(std::move(event));}
        previous.reset();drain();return {prepared.snapshot_,generation,prepared.operation_,true};
    }
    template<class Factory>CommitResult reload(Factory&& factory,const Cancellation& cancellation,PreparationOptions options={}){auto prepared=prepare(std::forward<Factory>(factory),cancellation,options);return commit(prepared,cancellation);}
    void clear(){detach(false);}void close(){detach(true);}
private:
    void detach(bool close){auto state=state_;std::shared_ptr<const Snapshot> previous;{std::lock_guard<std::mutex> lock(state->gate);if(state->closed){if(close)return;throw ConfigError(ErrorCode::ManagerClosed,"Manager closed");}advance(*state);if(close)state->closed.store(true,std::memory_order_release);previous=std::atomic_exchange(&state->current,std::shared_ptr<const Snapshot>{});state->pending=0;if(state->cancellation){state->cancellation->cancel();state->cancellation.reset();}enqueue(ManagerEvent{close?ManagerEventKind::Closed:ManagerEventKind::Cleared,state->generation,0,std::nullopt});}previous.reset();drain();}
};
} // namespace zrconfig
