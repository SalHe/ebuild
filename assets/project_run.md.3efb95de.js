import{_ as s,c as n,o as a,a as l}from"./app.8608cb56.js";var p="/ebuild/assets/run_show-envs.95b22f58.png",e="/ebuild/assets/run_cmd-args.43d584ac.png",o="/ebuild/assets/run_e-script.c63d3cb3.png";const _=JSON.parse('{"title":"\u5DE5\u7A0B\u811A\u672C","description":"","frontmatter":{},"headers":[{"level":2,"title":"\u5DE5\u7A0B\u76F8\u5173\u811A\u672C","slug":"\u5DE5\u7A0B\u76F8\u5173\u811A\u672C"},{"level":2,"title":"\u5411\u811A\u672C\u4F20\u9012\u53C2\u6570","slug":"\u5411\u811A\u672C\u4F20\u9012\u53C2\u6570"},{"level":2,"title":"\u4F7F\u7528\u6613\u8BED\u8A00\u7A0B\u5E8F\u4F5C\u4E3A\u811A\u672C","slug":"\u4F7F\u7528\u6613\u8BED\u8A00\u7A0B\u5E8F\u4F5C\u4E3A\u811A\u672C"}],"relativePath":"project/run.md","lastUpdated":1659975924000}'),c={name:"project/run.md"},t=l(`<h1 id="\u5DE5\u7A0B\u811A\u672C" tabindex="-1">\u5DE5\u7A0B\u811A\u672C <a class="header-anchor" href="#\u5DE5\u7A0B\u811A\u672C" aria-hidden="true">#</a></h1><p>\u4E5F\u8BB8\u60A8\u5728\u5DE5\u7A0B\u9700\u8981\u505A\u9664\u4E86\u7F16\u8BD1\u4E4B\u5916\u7684\u4E00\u4E9B\u91CD\u590D\u5DE5\u4F5C\uFF0C\u91CD\u590D\u505A\u7684\u8BDD\u7279\u522B\u70E6\u3002\u4E00\u822C\u53EF\u4EE5\u5C06\u8FD9\u4E9B\u91CD\u590D\u5DE5\u4F5C\u5199\u6210\u811A\u672C\uFF0C\u5728\u9700\u8981\u7684\u65F6\u5019\u6267\u884C\u4E00\u4E0B\u811A\u672C\u5373\u53EF\uFF0C\u5C31\u4E0D\u7528\u624B\u52A8\u64CD\u4F5C\u4E86\u3002<code>ebuild</code>\u521A\u597D\u96C6\u6210\u4E86\u4E00\u4E9B\u76F8\u5173\u7684\u529F\u80FD\u3002</p><h2 id="\u5DE5\u7A0B\u76F8\u5173\u811A\u672C" tabindex="-1">\u5DE5\u7A0B\u76F8\u5173\u811A\u672C <a class="header-anchor" href="#\u5DE5\u7A0B\u76F8\u5173\u811A\u672C" aria-hidden="true">#</a></h2><p>\u5728<code>ebuild.yaml</code>\u4E2D\u6709\u4E00\u4E2A\u540D\u4E3A<code>scripts</code>\u7684\u8282\u70B9\uFF0C\u5176\u4E0B\u53EF\u4EE5\u5B58\u653E\u82E5\u5E72\u811A\u672C\uFF0C\u53EF\u4EE5\u4F7F\u7528<a href="./../cli/ebuild_run.html"><code>ebuild run</code></a>\u6267\u884C\u3002\u60A8\u5927\u53EF\u5C06\u5DE5\u7A0B\u76F8\u5173\u7684\u4E00\u4E9B\u56FA\u5B9A\u64CD\u4F5C\u5199\u6210\u811A\u672C\u653E\u5230<code>scripts</code>\u4E2D\u3002<code>ebuild</code>\u5728\u6267\u884C\u8FD9\u4E9B\u811A\u672C\u7684\u65F6\u5019\uFF0C\u4F1A\u4F20\u9012\u4E00\u4E9B\u73AF\u5883\u53D8\u91CF</p><div class="language-yaml"><span class="copy"></span><pre><code><span class="line"><span style="color:#F07178;">project</span><span style="color:#89DDFF;">:</span></span>
<span class="line"><span style="color:#89DDFF;">  </span><span style="color:#676E95;font-style:italic;"># ......</span></span>
<span class="line"><span style="color:#F07178;">scripts</span><span style="color:#89DDFF;">:</span></span>
<span class="line"><span style="color:#A6ACCD;">  </span><span style="color:#F07178;">show-envs</span><span style="color:#89DDFF;">:</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;font-style:italic;">|</span></span>
<span class="line"><span style="color:#C3E88D;">    @echo off</span></span>
<span class="line"><span style="color:#C3E88D;">    echo EBuild=&quot;%EBUILD_EXECUTABLE_PATH%&quot;</span></span>
<span class="line"><span style="color:#C3E88D;">    echo \u6613\u8BED\u8A00=&quot;%ELANG_DIR%&quot;</span></span>
<span class="line"><span style="color:#C3E88D;">    echo Ecl=&quot;%ECL_DIR%&quot;</span></span>
<span class="line"><span style="color:#C3E88D;">    echo E2Txt=&quot;%E2Txt_DIR%&quot;</span></span>
<span class="line"><span style="color:#A6ACCD;">  </span><span style="color:#F07178;">get-input</span><span style="color:#89DDFF;">:</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;font-style:italic;">|</span></span>
<span class="line"><span style="color:#C3E88D;">    @echo off</span></span>
<span class="line"><span style="color:#C3E88D;">    @REM \u6F14\u793A\u5982\u4F55\u83B7\u53D6\u7528\u6237\u8F93\u5165</span></span>
<span class="line"><span style="color:#C3E88D;">    </span></span>
<span class="line"><span style="color:#C3E88D;">    set /p Username=\u7528\u6237\u540D\uFF1A</span></span>
<span class="line"><span style="color:#C3E88D;">    set /p Password=\u5BC6\u7801\uFF1A</span></span>
<span class="line"><span style="color:#C3E88D;">    </span></span>
<span class="line"><span style="color:#C3E88D;">    echo/</span></span>
<span class="line"><span style="color:#C3E88D;">    echo \u60A8\u7684\u7528\u6237\u540D\uFF1A%Username%</span></span>
<span class="line"><span style="color:#C3E88D;">    echo \u60A8\u7684\u5BC6\u7801\uFF1A%Password%</span></span>
<span class="line"><span style="color:#A6ACCD;">  </span><span style="color:#F07178;">cmd-args</span><span style="color:#89DDFF;">:</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;font-style:italic;">|</span></span>
<span class="line"><span style="color:#C3E88D;">    @echo off</span></span>
<span class="line"><span style="color:#C3E88D;">    echo arg0=%0</span></span>
<span class="line"><span style="color:#C3E88D;">    echo arg1=%1</span></span>
<span class="line"><span style="color:#C3E88D;">    echo arg2=%2</span></span>
<span class="line"><span style="color:#C3E88D;">    echo arg3=%3</span></span>
<span class="line"></span></code></pre></div><p>\u5728\u4E0A\u8FF0\u914D\u7F6E\u4E2D\u6211\u4EEC\u5B9A\u4E49\u4E86\u4E09\u4E2A\u811A\u672C\uFF1A<code>show-envs</code>\u3001<code>get-input</code>\u3001<code>cmd-args</code>\u3002\u5206\u522B\u6F14\u793A\u4E86\u8F93\u51FA<code>ebuild</code>\u4F20\u9012\u7684\u73AF\u5883\u53D8\u91CF\u3001\u4ECE\u6807\u51C6\u8F93\u5165\u83B7\u53D6\u7528\u6237\u8F93\u5165\u3001\u83B7\u53D6\u4F20\u9012\u7ED9\u811A\u672C\u7684\u547D\u4EE4\u884C\u53C2\u6570\u3002</p><p>\u8981\u6267\u884C\u4E0A\u8FF0\u811A\u672C\uFF0C\u6211\u4EEC\u53EA\u9700\u8981\u5728\u547D\u4EE4\u884C\u4E2D\u6267\u884C<code>ebuild run XXX</code>\u3002</p><p><img src="`+p+`" alt="show-envs"></p><h2 id="\u5411\u811A\u672C\u4F20\u9012\u53C2\u6570" tabindex="-1">\u5411\u811A\u672C\u4F20\u9012\u53C2\u6570 <a class="header-anchor" href="#\u5411\u811A\u672C\u4F20\u9012\u53C2\u6570" aria-hidden="true">#</a></h2><p>\u5982\u679C\u60A8\u8981\u60F3\u6240\u6267\u884C\u7684\u811A\u672C\u4F20\u9012\u53C2\u6570\uFF0C\u53EA\u9700\u8981\u6267\u884C\uFF1A</p><div class="language-shell"><span class="copy"></span><pre><code><span class="line"><span style="color:#A6ACCD;">ebuild run XXX -- \u547D\u4EE4\u884C\u53C2\u6570...</span></span>
<span class="line"></span></code></pre></div><p>\u5982\uFF1A</p><p><img src="`+e+`" alt="cmd-args"></p><h2 id="\u4F7F\u7528\u6613\u8BED\u8A00\u7A0B\u5E8F\u4F5C\u4E3A\u811A\u672C" tabindex="-1">\u4F7F\u7528\u6613\u8BED\u8A00\u7A0B\u5E8F\u4F5C\u4E3A\u811A\u672C <a class="header-anchor" href="#\u4F7F\u7528\u6613\u8BED\u8A00\u7A0B\u5E8F\u4F5C\u4E3A\u811A\u672C" aria-hidden="true">#</a></h2><p>\u4E5F\u8BB8\u60A8\u4E0D\u719F\u6089<code>bat</code>\u811A\u672C\u7684\u7F16\u5199\uFF0C\u90A3\u4E48\u60A8\u53EF\u4EE5\u9009\u62E9\u4F7F\u7528\u6613\u8BED\u8A00\u6765\u7F16\u5199\u60A8\u7684\u811A\u672C\u3002\u7136\u540E\u4F7F\u7528<code>ebuild</code>\u6765\u8FD0\u884C(run)\u60A8\u7684\u6E90\u6587\u4EF6\u5373\u53EF\u3002</p><p>\u6BD4\u5982\u5F53\u524D\u5DE5\u7A0B\u6709\u6E90\u6587\u4EF6<a href="https://github.com/SalHe/ebuild/blob/4f53059ce09148ee58821b8460b8cef8e6bbd18e/examples/simple/scripts/%E6%98%93%E8%AF%AD%E8%A8%80%E5%81%9A%E8%84%9A%E6%9C%AC%E7%A4%BA%E4%BE%8B.e" target="_blank" rel="noopener noreferrer"><code>&lt;\u5DE5\u7A0B\u6839\u76EE\u5F55&gt;\\scripts\\\u6613\u8BED\u8A00\u505A\u811A\u672C\u793A\u4F8B.e</code></a>\u3002\u5176\u4EE3\u7801\u5982\u4E0B\uFF1A</p><div class="language-"><span class="copy"></span><pre><code><span class="line"><span style="color:#A6ACCD;">.\u7248\u672C 2</span></span>
<span class="line"><span style="color:#A6ACCD;"></span></span>
<span class="line"><span style="color:#A6ACCD;">.\u7A0B\u5E8F\u96C6 \u7A0B\u5E8F\u96C61</span></span>
<span class="line"><span style="color:#A6ACCD;"></span></span>
<span class="line"><span style="color:#A6ACCD;">.\u5B50\u7A0B\u5E8F _\u542F\u52A8\u5B50\u7A0B\u5E8F, \u6574\u6570\u578B, , \u672C\u5B50\u7A0B\u5E8F\u5728\u7A0B\u5E8F\u542F\u52A8\u540E\u6700\u5148\u6267\u884C</span></span>
<span class="line"><span style="color:#A6ACCD;">.\u5C40\u90E8\u53D8\u91CF \u547D\u4EE4\u884C\u53C2\u6570, \u6587\u672C\u578B, , &quot;0&quot;</span></span>
<span class="line"><span style="color:#A6ACCD;">.\u5C40\u90E8\u53D8\u91CF i, \u6574\u6570\u578B</span></span>
<span class="line"><span style="color:#A6ACCD;"></span></span>
<span class="line"><span style="color:#A6ACCD;"></span></span>
<span class="line"><span style="color:#A6ACCD;">\u6807\u51C6\u8F93\u51FA (, \u201C\u4F60\u597D\uFF0C\u8FD9\u662F\u811A\u672C\uFF01\u201D \uFF0B #\u6362\u884C\u7B26)</span></span>
<span class="line"><span style="color:#A6ACCD;">\u6807\u51C6\u8F93\u51FA (, \u201C\u5B83\u4F1A\u88AB\u81EA\u52A8\u7F16\u8BD1\u5E76\u6267\u884C\uFF01\u201D \uFF0B #\u6362\u884C\u7B26)</span></span>
<span class="line"><span style="color:#A6ACCD;">\u6807\u51C6\u8F93\u51FA (, #\u6362\u884C\u7B26)</span></span>
<span class="line"><span style="color:#A6ACCD;"></span></span>
<span class="line"><span style="color:#A6ACCD;">\u6807\u51C6\u8F93\u51FA (, \u201CEbuild: \u201D \uFF0B \u8BFB\u73AF\u5883\u53D8\u91CF (\u201CEBUILD_EXECUTABLE_PATH\u201D) \uFF0B #\u6362\u884C\u7B26)</span></span>
<span class="line"><span style="color:#A6ACCD;">\u6807\u51C6\u8F93\u51FA (, \u201C\u6613\u8BED\u8A00: \u201D \uFF0B \u8BFB\u73AF\u5883\u53D8\u91CF (\u201CELANG_DIR\u201D) \uFF0B #\u6362\u884C\u7B26)</span></span>
<span class="line"><span style="color:#A6ACCD;">\u6807\u51C6\u8F93\u51FA (, \u201CEcl: \u201D \uFF0B \u8BFB\u73AF\u5883\u53D8\u91CF (\u201CECL_DIR\u201D) \uFF0B #\u6362\u884C\u7B26)</span></span>
<span class="line"><span style="color:#A6ACCD;">\u6807\u51C6\u8F93\u51FA (, \u201CE2Txt: \u201D \uFF0B \u8BFB\u73AF\u5883\u53D8\u91CF (\u201CE2TXT_DIR\u201D) \uFF0B #\u6362\u884C\u7B26)</span></span>
<span class="line"><span style="color:#A6ACCD;">\u6807\u51C6\u8F93\u51FA (, #\u6362\u884C\u7B26)</span></span>
<span class="line"><span style="color:#A6ACCD;"></span></span>
<span class="line"><span style="color:#A6ACCD;">\u6807\u51C6\u8F93\u51FA (, \u201C\u547D\u4EE4\u884C\u53C2\u6570\uFF1A\u201D \uFF0B #\u6362\u884C\u7B26)</span></span>
<span class="line"><span style="color:#A6ACCD;">\u53D6\u547D\u4EE4\u884C (\u547D\u4EE4\u884C\u53C2\u6570)</span></span>
<span class="line"><span style="color:#A6ACCD;">.\u8BA1\u6B21\u5FAA\u73AF\u9996 (\u53D6\u6570\u7EC4\u6210\u5458\u6570 (\u547D\u4EE4\u884C\u53C2\u6570), i)</span></span>
<span class="line"><span style="color:#A6ACCD;">    \u6807\u51C6\u8F93\u51FA (, \u547D\u4EE4\u884C\u53C2\u6570 [i] \uFF0B #\u6362\u884C\u7B26)</span></span>
<span class="line"><span style="color:#A6ACCD;">.\u8BA1\u6B21\u5FAA\u73AF\u5C3E ()</span></span>
<span class="line"><span style="color:#A6ACCD;">\u6807\u51C6\u8F93\u51FA (, #\u6362\u884C\u7B26)</span></span>
<span class="line"><span style="color:#A6ACCD;"></span></span>
<span class="line"><span style="color:#A6ACCD;">\u6807\u51C6\u8F93\u51FA (, \u201C\u5DE5\u7A0B\u6839\u8DEF\u5F84\uFF1A\u201D \uFF0B \u8BFB\u73AF\u5883\u53D8\u91CF (\u201CEBUILD_PROJECT_ROOT_DIR\u201D) \uFF0B #\u6362\u884C\u7B26)</span></span>
<span class="line"><span style="color:#A6ACCD;">\u6807\u51C6\u8F93\u51FA (, \u201C\u5DE5\u7A0B\u8F93\u51FA\u8DEF\u5F84\uFF1A\u201D \uFF0B \u8BFB\u73AF\u5883\u53D8\u91CF (\u201CEBUILD_PROJECT_OUTPUT_DIR\u201D) \uFF0B #\u6362\u884C\u7B26)</span></span>
<span class="line"><span style="color:#A6ACCD;">\u6807\u51C6\u8F93\u51FA (, #\u6362\u884C\u7B26)</span></span>
<span class="line"><span style="color:#A6ACCD;"></span></span>
<span class="line"><span style="color:#A6ACCD;">\u8FD4\u56DE (0)  &#39; \u53EF\u4EE5\u6839\u636E\u60A8\u7684\u9700\u8981\u8FD4\u56DE\u4EFB\u610F\u6570\u503C</span></span>
<span class="line"><span style="color:#A6ACCD;"></span></span></code></pre></div><p>\u60A8\u53EF\u4EE5\u6267\u884C\u4EE5\u4E0B\u547D\u4EE4\u6765&quot;\u6267\u884C&quot;\u6613\u8BED\u8A00\u6E90\u6587\u4EF6\uFF1A</p><div class="language-shell"><span class="copy"></span><pre><code><span class="line"><span style="color:#A6ACCD;">ebuild run ./scripts/\u6613\u8BED\u8A00\u505A\u811A\u672C\u793A\u4F8B.e -- \u4F20\u9012\u7ED9\u6613\u8BED\u8A00\u7A0B\u5E8F\u7684\u547D\u4EE4\u884C\u53C2\u6570...</span></span>
<span class="line"></span></code></pre></div><p>\u6BD4\u5982\uFF1A</p><p><img src="`+o+'" alt="&quot;\u6267\u884C&quot;\u6613\u8BED\u8A00\u6E90\u6587\u4EF6"></p><div class="tip custom-block"><p class="custom-block-title">ebuild\u662F\u5982\u4F55\u6267\u884C\u6E90\u6587\u4EF6\u7684\uFF1F</p><p>\u5176\u5B9E<code>ebuild</code>\u53EA\u662F\u81EA\u52A8\u4F7F\u7528<code>ecl</code>\u5B8C\u6210\u4E86\u5C06\u60A8\u6240\u6307\u5B9A\u7684\u6E90\u7801\u7F16\u8BD1\u6210\u53EF\u6267\u884C\u6587\u4EF6\u7684\u8FC7\u7A0B\uFF0C\u5728\u4E0A\u9762\u7684\u56FE\u4E2D\u6211\u4EEC\u4E5F\u53EF\u4EE5\u770B\u5230<code>ebuild</code>\u7F16\u8BD1\u6E90\u6587\u4EF6\u7684\u63D0\u793A\u3002</p><p>\u56E0\u4E3A&quot;\u6267\u884C&quot;\u8FC7\u7A0B\u4E2D\u9700\u8981\u7F16\u8BD1\u6E90\u6587\u4EF6\uFF0C\u6240\u4EE5\u5EFA\u8BAE\u60A8\u7684\u6E90\u6587\u4EF6\u4E0D\u5E94\u8BE5\u592A\u5927\u3002</p></div>',22),r=[t];function i(d,C,A,D,y,u){return a(),n("div",null,r)}var h=s(c,[["render",i]]);export{_ as __pageData,h as default};