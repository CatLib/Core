# Release Notes

## [v2.0.0-alpha.1](https://github.com/CatLib/Core/releases/tag/v2.0.0-alpha.1)

#### Added

- [x] `Inject` allowed to be set to optional.(#253 )

#### Changed

- [x] Comments translated from Chinese into English(#133 )
- [x] Defined Container.Build as a virtual function(#210 )
- [x] Optimizes the constructor of `MethodContainer`(#218 )
- [x] The default project uses the .net standard 2.0(#225 )
- [x] Rename Util helper class to Helper class Change access level to internal.(#230 )
- [x] `Application.IsRegisted` changed(rename) to `IsRegistered`(#226 ) 
- [x] Use `VariantAttribute` to mark variable types instead of `IVariant`(#232 )
- [x] `Guard` Will be expandable with `Guard.That`(#233 )
- [x] Fixed the problem of container exception stack loss(#234 )
- [x] Adjusted the internal file structure to make it clearer(#236 ).
- [x] Add code analyzers (#206 )
- [x] Refactoring event system (#177 )
- [x] Refactoring `RingBuffer` make it inherit from `Stream`.(#238 )
- [x] Namespace structure adjustment(optimization).(#241 )
- [x] `App` can be extended by `That` (Handler rename to that) and removed `HasHandler` API (#242 )
- [x] Unnecessary inheritance: WrappedStream(#247 )
- [x] Clean up useless comment(#249 ).
- [x] `Guard.Require` can set error messages and internal exceptions(#250).
- [x] Exception class implementation support: SerializationInfo build(#252 ).
- [x] Refactoring unit test, import moq.(#255 )
- [x] `CodeStandardException` replaces to `LogicException`(#257 )
- [x] Exception move to namespace `CatLib.Exception`(#258 )
- [x] `Facade<>.Instance` changed to `Facade<>.That`(#259 )
- [x] `Application.StartProcess` migrate to `StartProcess`(#260 )
- [x] `Arr` optimization, lifting some unnecessary restrictions (#263)
- [x] `Str` optimization, lifting some unnecessary restrictions (#264)
- [x] Refactoring `SortSet`(#265 )
- [x] Removed global params in application constructor. use Application.New() instead.(#267 )
- [x] Containers are no longer thread-safe by default(#270 )

#### Fixed

- [x] Fixed a bug that caused `Arr.Fill` to not work properly under special circumstances. (#255 )

#### Removed

- [x] Removed `ExcludeFromCodeCoverageAttribute` (#229 )
- [x] Removed unnecessary interface design `ISortSet`(#211 ).
- [x] Removed `Version` classes and `Application.Compare` method.(#212).
- [x] Removed `Template`  supported(#213 ).
- [x] Removed `FilterChain` supported(#214 ).
- [x] Removed `Enum` supported(#215 ).
- [x] Removed `IAwait` interface(#217 ).
- [x] Removed `Container.Flash`  api(#219 ).
- [x] Removed `Arr.Flash` method(#220 ).
- [x] Removed `Dict` helper class(#221 ).
- [x] Removed `ThreadStatic` helper class(#223 ).
- [x] Removed `QuickList` supported(#224 ).
- [x] Removed `Storage` supported(#228 )
- [x] Removed `SystemTime` class(#235 ).
- [x] Removed `ICoroutineInit` feature from core library(#243 ).
- [x] Removed the priority attribute, depending on the loading order(#244 ).
- [x] Removed `Util.Encoding` (#245 ).
- [x] Removed `Str.Encoding`(#246 )
- [x] Removed `IServiceProviderType` feature in core library(#246 ).
- [x] Removed unnecessary extension functions(#247 ).
- [x] Removed `PipelineStream` stream.(#256 )
- [x] Removed all `Obsolete` method and clean code.(#261 )
- [x] Removed `App.Version`.(#266 )