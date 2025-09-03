# HybridCLRCrashWorkarounds

本工具用于规避HybridCLR引发的Unity崩溃问题，详见[文章](https://alanliu90.hatenablog.com/entry/2023/12/22/%E6%8E%92%E6%9F%A5HybridCLR%E5%BC%95%E5%8F%91%E7%9A%84%E5%B4%A9%E6%BA%83%E9%97%AE%E9%A2%98)

支持平台:
* Android
* iOS

## 集成

1. 引用包com.modx.hybridclr-crash-workarounds
2. 打包时将热更dll中的`MonoScript`的信息写入一个文件，可参考 demo\HybridCLRTrial\Assets\Editor\MyMonoScriptWriter.cs
3. 打包时记录符号的偏移，可参考 demo\HybridCLRTrial\Assets\Editor\SymbolOffsetsWriter.cs
4. 运行时加载记录`MonoScript`的信息的文件和记录符号的偏移的文件，调用工具的接口创建`MonoScript`，可参考 demo\HybridCLRTrial\Assets\HotUpdate\Test.cs

### 说明:
1. 只有Android需要记录符号的偏移，如果只对iOS使用，可以跳过第三步
2. 第二步生成的文件的内容是对应热更dll的，需要热更
3. 第三步生成的文件的内容是对应包体的，不需要热更
4. 调用工具的接口创建`MonoScript`，需要在HybridCLR加载热更dll之后
5. 工具提供了2种方式收集`MonoScript`：反射和使用dnlib解析。两者的区别:
	* 反射: 需要自己过滤掉编辑器内的`MonoBehaviour`和`ScriptableObject`(比如定义了`UNITY_EDITOR`才启用的类型)。优点是对执行时机没有要求
	* dnlib解析: 由于是从打包编译的dll中收集`MonoScript`，所以不会有编辑器内的`MonoBehaviour`和`ScriptableObject`。这种方法用到了最新版本的AOT dll，需要有合适的时机执行，比如在`HybridCLR/Generate/All`之后执行

### 兼容旧包
对于线上已有包体的情况，可以在热更dll中这样实现：
```C#
private void CreateMonoScripts()
{
	if (/* 判断是新包 */)
		CreateMonoScriptsInternal();
}

private void CreateMonoScriptsInternal()
{
	var bytes = /* 加载 MonoScripts.bytes */;
	HybridCLRCrashWorkarounds.CreateMonoScripts(bytes);
}
```

### 性能
对于3757个`MonoScript`的情况，使用Unity 2020.3.60f1测试`HybridCLRCrashWorkarounds.CreateMonoScripts`的耗时：
* Redmi 8A: 约750ms
* Xiaomi Mi 10: 约180ms
* iPad 9th generation: 约90ms

## 崩溃调用栈例子
1. 该调用栈是本地复现工程在Android设备上出现的(Unity 2022.3.60f1)，确认集成本工具后不再出现
```
#00 pc 0000000001337f3c (std::__ndk1::__shared_weak_count::__release_weak() at /buildbot/src/android/ndk-release-r23\toolchain/llvm-project/libcxx/src/memory.cpp:0) 
#01 pc 00000000008dbc70 (FindOrCreateMonoScriptCache(ScriptingClassPtr, InitScriptingCacheType, Object*, int, core::basic_string<char, core::StringStorageDefault<char> >) at ??:0)
#02 pc 00000000008dbb44 (MonoScript::Renew(ScriptingClassPtr) at ??:0)
#03 pc 0000000000921cac (PersistentManager::PostReadActivationQueue(int, TypeTree const*, bool, PersistentManager::LockFlags) at ??:0)
#04 pc 000000000092240c (PersistentManager::ReadAndActivateObjectThreaded(int, SerializedObjectIdentifier const&, SerializedFile*, bool, bool, PersistentManager::LockFlags) at ??:0)
#05 pc 0000000000922618 (PersistentManager::LoadObjectsThreaded(int const*, int, LoadProgress&, bool, PersistentManager::LockFlags, bool) at ??:0)
#06 pc 00000000007d63b8 (LoadOperation::Perform() at ??:0)
#07 pc 0000000000adcf94 (AssetBundleLoadAssetOperation::Perform() at ??:0)
#08 pc 00000000007d8838 (PreloadManager::ProcessSingleOperation() at ??:0)
#09 pc 00000000007d85c0 (PreloadManager::Run() at ??:0)
#10 pc 00000000007d8518 (PreloadManager::Run(void*) at ??:0)
#11 pc 000000000087bfb8 (Thread::RunThreadWrapper(void*) at ??:0)
```

2. 该调用栈是某个项目线上在iOS设备上出现的(Unity 2022.3.57f1)，目前还在等该项目集成本工具
```
1 lookup<long, core::equal_pair<std::equal_to<long>, long, std::weak_ptr<MonoScriptCache> > > (./Runtime/Core/Containers/hash_set.h:948)
2 lookup<long, core::equal_pair<std::equal_to<long>, long, std::weak_ptr<MonoScriptCache> > > (./Runtime/Core/Containers/hash_set.h:943)
3 GetMonoScriptCache (./Runtime/Core/Containers/hash_map.h:210)
4 FindOrCreateMonoScriptCache (./Runtime/Mono/MonoScriptCache.cpp:625)
5 SetupScriptingCache (./Runtime/Scripting/ManagedReference/SerializableManagedRef.cpp:190)
6 RebuildMonoInstance (./Runtime/Scripting/ManagedReference/SerializableManagedRef.cpp:222)
7 RebuildMonoInstanceFromScriptChange (./Runtime/Scripting/ManagedReference/SerializableManagedRef.cpp:282)
8 RebuildMonoInstanceFromScriptChange (./Runtime/Mono/ManagedMonoBehaviourRef.cpp:306)
9 SetScript (./Runtime/Scripting/ManagedReference/SerializableManagedRef.cpp:418)
10 ProduceClone (./Runtime/GameCode/CloneObject.cpp:62)
11 CollectAndProduceGameObjectHierarchy (./Runtime/GameCode/CloneObject.cpp:89)
12 CollectAndProduceClonedIsland (./Runtime/GameCode/CloneObject.cpp:235)
13 CloneObjectImpl (./Runtime/GameCode/CloneObject.cpp:308)
14 CloneObject (./Runtime/GameCode/CloneObject.cpp:566)
15 Object_CUSTOM_Internal_CloneSingleWithParent (artifacts/iOS/Modules/iOS_arm64_nondev_i_r/Bindings/CoreBindings.gen.cpp:62420)
16 Object_Instantiate_m99F2A72EF6BFE09E6CF4FCF6207C5BCFAD1D76CF
...
```
