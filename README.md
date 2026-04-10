# logs_filter

## 目的

这个项目是为了制作一个工具可以快速从`NavigationService.g3log.20251124-200157.37951.log`这类日志中提取出符合特定格式要求的记录并保存到对应的`can_service.g3log.20251124-200157.37951.log`文件中，文件名中的`.g3log.20251124-200157.37951.log`保持与输入文件一样，只需要从`NavigationService`另存为`can_service`。

## 匹配公式

\d{2}:\d{2}:\d{2} \d{3}\(\d{4}\).*[^0-9a-fA-F][^0-9a-fA-F](?:\s*[0-9a-fA-F]{1,2}){1}(?:\s[0-9a-fA-F]{1,2}){7}(?:\s*|\s*,.*|\s\[(?:[0-9a-fA-F]{1,2}(?:\s[0-9a-fA-F]{1,2})*)\])$

## 搜寻公式

\d{2}:\d{2}:\d{2} \d{3}\(\d{4}\).*[^0-9a-fA-F][^0-9a-fA-F](?:\s*[0-9a-fA-F]{1,2}){1}(?:\s[0-9a-fA-F]{1,2}){7}

## 编译

```bash
dotnet publish -c Release -r win-x64 --self-contained
```
