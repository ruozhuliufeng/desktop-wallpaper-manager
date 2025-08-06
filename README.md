# Desktop Wallpaper Manager

一个轻量级的Windows桌面壁纸管理应用，支持自定义桌面组件（时钟、番茄时钟等），组件可拖拽移动，透明度可调整。

## 功能特性

- ✅ **壁纸管理**: 支持图片和视频作为桌面背景
- ✅ **可移动组件**: 时钟组件，支持拖拽移动
- ✅ **番茄时钟**: 完整的番茄工作法计时器
- ✅ **透明度调节**: 组件透明度可调整（0.1-1.0）
- ✅ **布局保存**: 支持保存和加载组件布局
- ✅ **轻量级设计**: 优化内存使用，占用资源少
- ✅ **桌面覆盖**: 可以隐藏主窗口，组件直接显示在桌面上

## 系统要求

- Windows 10/11
- .NET 8.0 Runtime
- 内存需求: 约 20-50MB

## 构建说明

### 前置要求
- Visual Studio 2022 或 .NET 8.0 SDK
- Windows 操作系统

### 构建步骤

1. 克隆或下载项目文件到本地
2. 在项目根目录打开命令提示符
3. 运行构建命令：

```bash
cd DesktopWallpaperApp/src/DesktopWallpaperApp
dotnet build --configuration Release
```

### 运行应用

```bash
dotnet run --configuration Release
```

或者构建可执行文件：

```bash
dotnet publish --configuration Release --self-contained true --runtime win-x64
```

构建完成后，可执行文件位于：
`bin/Release/net8.0-windows/win-x64/publish/DesktopWallpaperApp.exe`

## 使用说明

### 基本操作

1. **设置壁纸**: 点击"选择壁纸"按钮，选择图片文件
2. **添加组件**: 使用"添加时钟"或"添加番茄时钟"按钮
3. **移动组件**: 单击并拖拽组件到想要的位置
4. **调整透明度**: 使用侧边栏的透明度滑块
5. **隐藏主窗口**: 点击"隐藏窗口"，组件将显示在桌面上

### 组件功能

#### 时钟组件
- 显示当前时间和日期
- 支持拖拽移动
- 双击可打开设置（待开发）

#### 番茄时钟组件  
- 标准番茄工作法：25分钟工作，5分钟短休息，15分钟长休息
- 支持开始/暂停/停止操作
- 自动切换工作和休息状态
- 完成时播放提示音

### 快捷操作

- 右键点击组件区域可快速添加组件
- 双击组件可打开设置
- 支持布局的保存和加载

## 项目结构

```
DesktopWallpaperApp/
├── src/
│   └── DesktopWallpaperApp/
│       ├── Controls/           # 自定义控件
│       │   ├── ClockWidget.xaml
│       │   └── PomodoroWidget.xaml
│       ├── Models/             # 数据模型
│       │   ├── WidgetModel.cs
│       │   └── PomodoroModel.cs
│       ├── Services/           # 服务层
│       │   ├── WallpaperService.cs
│       │   └── MediaService.cs
│       ├── Utils/              # 工具类
│       │   ├── SettingsManager.cs
│       │   └── PerformanceOptimizer.cs
│       ├── Views/              # 视图
│       │   ├── MainWindow.xaml
│       │   └── OverlayWindow.xaml
│       └── App.xaml
```

## 性能优化

应用采用了多种性能优化策略：

1. **内存管理**: 定时垃圾收集，限制工作集大小
2. **低内存模式**: 可选的低内存占用模式
3. **进程优先级**: 设置为较低优先级，避免影响其他应用
4. **设置持久化**: 自动保存和恢复应用设置
5. **单例模式**: 确保只运行一个实例

## 开发计划

- [ ] 添加更多组件类型（天气、系统监控等）
- [ ] 支持组件主题和样式自定义
- [ ] 添加系统托盘功能
- [ ] 支持视频壁纸播放
- [ ] 添加组件动画效果
- [ ] 云同步布局配置

## 故障排除

### 常见问题

1. **应用无法启动**: 确保已安装.NET 8.0 Runtime
2. **壁纸设置失败**: 检查图片文件权限，尝试以管理员身份运行
3. **组件不显示**: 检查透明度设置，确保不为完全透明
4. **内存占用过高**: 启用低内存模式，定期重启应用

### 调试模式

在Debug模式下，状态栏会显示实时内存使用情况，方便监控性能。

## 技术栈

- **框架**: WPF (.NET 8.0)
- **语言**: C# 12
- **架构**: MVVM模式
- **依赖包**: 
  - Microsoft.Win32.Registry
  - System.Drawing.Common

## 许可证

本项目仅供学习和个人使用。

## 贡献

欢迎提交Issue和Pull Request来改进这个项目。

---

**注意**: 这是一个轻量级的桌面工具，设计目标是占用最少的系统资源同时提供实用的功能。