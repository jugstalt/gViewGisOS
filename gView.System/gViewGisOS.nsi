;--------------------------------
;Include Modern UI

  !include "MUI2.nsh"
  !include "Sections.nsh"
  !include "LogicLib.nsh"
  !include "Memento.nsh"
  !include "WordFunc.nsh"

;--------------------------------
;!include LogicLib.nsh
!include "DotNet.nsh"
!define DOTNET_VERSION "2.0.50727"
!include "registerExtension.nsh"
!include "x64.nsh"

;General
  
 ; name and file
  Name "gView GIS OS"
  OutFile "gViewGisOS.exe"
  
  BrandingText "gView GIS OS Install"
  
  ; The default installation directory
  InstallDir $PROGRAMFILES\gViewGisOS
  
  ;Get installation folder from registry if available
  InstallDirRegKey HKLM "Software\gViewGisOS" "Install_Dir"

  ;Request application privileges for Windows Vista
  RequestExecutionLevel admin

;--------------------------------
;Interface Settings

  !define MUI_HEADERIMAGE
  !define MUI_HEADERIMAGE_BITMAP "${NSISDIR}\Contrib\Graphics\Header\orange.bmp"
  !define MUI_HEADERIMAGE_UNBITMAP "${NSISDIR}\Contrib\Graphics\Header\orange-uninstall.bmp"
  !define MUI_WELCOMEFINISHPAGE_BITMAP ".\img\wizard_left.bmp"
  !define MUI_UNWELCOMEFINISHPAGE_BITMAP ".\img\wizard_left.bmp"
  
  !define MUI_ABORTWARNING
  !define MUI_ICON ".\img\new\map.ico"
  !define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\arrow2-uninstall.ico"
    
;--------------------------------
;Language Selection Dialog Settings

  ;Remember the installer language
  !define MUI_LANGDLL_REGISTRY_ROOT "HKLM" 
  !define MUI_LANGDLL_REGISTRY_KEY "Software\gViewGisOS" 
  !define MUI_LANGDLL_REGISTRY_VALUENAME "Installer Language"
  
;--------------------------------
;Variables

  Var StartMenuFolder
  
;--------------------------------
;Pages

  !define MUI_COMPONENTSPAGE_NODESC

  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE "bin\doc\license.rtf"
  Page custom getUsername
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_STARTMENU Application $StartMenuFolder
  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro MUI_PAGE_FINISH
  
  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_UNPAGE_FINISH
  
;--------------------------------
;Languages

  !insertmacro MUI_LANGUAGE "English" ;first language is the default language
  !insertmacro MUI_LANGUAGE "German"
  
;--------------------------------
;Reserve Files
  
  ;If you are using solid compression, files that are required before
  ;the actual installation should be stored first in the data block,
  ;because this will make your installer start faster.
  
  !insertmacro MUI_RESERVEFILE_LANGDLL
  

;--------------------------------
;Installer Functions

Var UserName
Var Company 

Function .onInit

  ${If} ${RunningX64}
    SetRegView 64
  ${Else}
    
  ${EndIf}
  
	# the plugins dir is automatically deleted when the installer exits
	InitPluginsDir
	File /oname=$PLUGINSDIR\splash.bmp ".\img\splash1_install.bmp"
	
	splash::show 3000 $PLUGINSDIR\splash

	Pop $0 ; $0 has '1' if the user closed the splash screen early,
			; '0' if everything closed normally, and '-1' if some error occurred.
			
	!insertmacro MUI_LANGDLL_DISPLAY
	
	!insertmacro CheckDotNET ${DOTNET_VERSION}
	
  ;File /oname=$PLUGINSDIR\namecompany.ini "namecompany.ini"

  ;ReadRegStr "$UserName" HKLM SOFTWARE\gView "username" 
  ;ReadRegStr "$Company" HKLM SOFTWARE\gView "companyname"
  
  ;WriteINIStr "$PLUGINSDIR\namecompany.ini" "Field 2" "state" "$UserName" 
  ;WriteINIStr "$PLUGINSDIR\namecompany.ini" "Field 4" "state" "$Company"
  
FunctionEnd



  
Function getUsername
  Push $R0
  InstallOptions::dialog $PLUGINSDIR\namecompany.ini
  Pop $R0
  
  
  
  ReadINIStr $UserName "$PLUGINSDIR\namecompany.ini" "Field 2" "state"
  ReadINIStr $Company "$PLUGINSDIR\namecompany.ini" "Field 4" "state"
  
  Pop $R0
FunctionEnd



;--------------------------------
;Installer Sections



;--------------------------------

; The stuff to install
Section "gView Core"

  SectionIn RO
  
  SetOutPath "$INSTDIR"
  
  File "bin\gView.Setup.Uninstall.exe"
  File "bin\gView.Desktop.MapServer.Admin.exe"
  ExecWait "$INSTDIR\gView.Setup.Uninstall.exe"
  
  ; #####################################
  ; Write the installation path into the registry
  ; #####################################
  
  WriteRegStr HKLM SOFTWARE\gViewGisOS "Install_Dir" "$INSTDIR"
  ;WriteRegStr HKLM SOFTWARE\gView "username" "$UserName"
  ;WriteRegStr HKLM SOFTWARE\gView "companyname" "$Company" 
   
  ; ##################################### 
  ; Write the uninstall keys for Windows
  ; #####################################
  
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\gViewGisOS" "DisplayName" "gView GIS OS"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\gViewGisOS" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\gViewGisOS" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\gViewGisOS" "NoRepair" 1
  ;WriteRegStr HKLM "Software\Microsoft\.NETFramework\AssemblyFolders\gViewGisOS" "" "$INSTDIR\Framework"
  
  CreateDirectory "$INSTDIR"
  WriteUninstaller "$INSTDIR\uninstall.exe"
  
  ; ####################################
  ; config
  ; ####################################
  CreateDirectory "$INSTDIR\config"
  CreateDirectory "$INSTDIR\config\explorer"
  CreateDirectory "$INSTDIR\config\explorer\tmp"
  CreateDirectory "$INSTDIR\config\templates"
  
  SetOutPath "$INSTDIR\config\templates"
  File "bin\gView.MapServer.Instance.exe.config"
  ; File "bin\gView.MapServerAdmin.exe.config"
  
  ; ####################################
  ; Cursors
  ; ####################################
  CreateDirectory "$INSTDIR\Cursors"
  
  SetOutPath "$INSTDIR\Cursors"
  File "bin\Cursors\Rotation.cur"
  File "bin\Cursors\Vertex.cur"
  
  ; ####################################
  ; de
  ; ####################################
  CreateDirectory "$INSTDIR\de"
  
  SetOutPath "$INSTDIR\de"
  File "bin\de\gView.DataSources.Fdb.UI.resources.dll"
  File "bin\de\gView.DataSources.PostGIS.UI.resources.dll"
  File "bin\de\gView.Carto.Rendering.UI.resources.dll"
  File "bin\de\gView.DB.UI.resources.dll"
  File "bin\de\gView.Dialogs.resources.dll"
  File "bin\de\gView.Explorer.UI.resources.dll"
  File "bin\de\gView.Globalisation.resources.dll"
  File "bin\de\gView.Interoperability.ArcXML.UI.resources.dll"
  File "bin\de\gView.Interoperability.AGS.UI.resources.dll"
  File "bin\de\gView.Interoperability.OGC.UI.resources.dll"
  File "bin\de\gView.Interoperability.Sde.UI.a10.resources.dll"
  File "bin\de\gView.MapServer.Lib.UI.resources.dll"
  File "bin\de\gView.Plugins.Snapping.resources.dll"
  File "bin\de\gView.Plugins.DbTools.resources.dll"
  File "bin\de\gView.Plugins.Editor.resources.dll"
  File "bin\de\gView.Plugins.Tools.resources.dll"
  File "bin\de\gView.Symbology.UI.resources.dll"
  File "bin\de\gView.system.UI.resources.dll"
  
  ; ####################################
  ; doc
  ; ####################################
  CreateDirectory "$INSTDIR\doc"
  
  SetOutPath "$INSTDIR\doc"
  File "bin\doc\LICENSE-crypto.txt"
  File "bin\doc\LICENSE-dlmalloc.txt"
  File "bin\doc\LICENSE-gdal.txt"
  File "bin\doc\LICENSE-Lizardtech.txt"
  File "bin\doc\LICENSE-AvalonDock.txt"
  File "bin\doc\LICENSE-Fluent.txt"
  File "bin\doc\LICENSE.rtf"
  File "bin\doc\License.txt"
  File "bin\doc\README-3rdparty.txt"
  File "bin\doc\LICENSE-npgsql.txt"
  File "bin\doc\gView 4.0 Handbuch.pdf"
  File "bin\doc\Whitepaper - gView Daten in ArcMap laden.pdf"
  File "bin\doc\Whitepaper - Ortsplan erstellen.pdf"
  File "bin\doc\Whitepaper - Migration von ArcXML Diensten.pdf"
  File "bin\doc\Whitepaper - Joins & Views.pdf"
  File "bin\doc\Whitepaper - OGR Simple Feature Library.pdf"
  
  ; ####################################
  ; Framework
  ; ####################################
  CreateDirectory "$INSTDIR\Framework"
  
  SetOutPath "$INSTDIR\Framework"
  
  ${If} ${RunningX64}
     WriteRegStr HKLM "SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\gViewGisOS" "" "$INSTDIR\Framework"
  ${Else}
     WriteRegStr HKLM "SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\gViewGisOS" "" "$INSTDIR\Framework"
  ${EndIf}
  
  File "bin\gView.Core.dll"                         
  File "bin\gView.Carto.dll"                        
  File "bin\gView.Core.UI.dll"                      
  File "bin\gView.Data.dll"                         
  File "bin\gView.DB.dll"                           
  File "bin\gView.DB.UI.dll"                        
  File "bin\gView.Editor.Core.dll"                  
  File "bin\gView.Geometry.dll"                     
  File "bin\gView.MapServer.Connector.dll"          
  File "bin\gView.Math.dll"                         
  File "bin\gView.Snapping.Core.dll"                
  File "bin\gView.system.dll"                       
  File "bin\gView.system.UI.dll"                    
  File "bin\gView.UI.dll"                           
  File "bin\gView.XML.dll" 
  File "bin\gView.Offline.dll"                       
  
  ; ####################################
  ; sql
  ; ####################################
  CreateDirectory "$INSTDIR\sql"
  CreateDirectory "$INSTDIR\sql\accessFDB"
  CreateDirectory "$INSTDIR\sql\sqlFDB"
  CreateDirectory "$INSTDIR\sql\postgreFDB"
  CreateDirectory "$INSTDIR\sql\SQLiteFDB"
  
  SetOutPath "$INSTDIR\sql\accessFDB"
  File "bin\sql\accessFDB\fdb_1_0_0.tpl"
  File "bin\sql\accessFDB\fdb_1_2_0.tpl"
  
  SetOutPath "$INSTDIR\sql\sqlFDB"
  File "bin\sql\sqlFDB\createdatabase.sql"
  File "bin\sql\sqlFDB\createdatabase_from_mdf.sql"
  File "bin\sql\sqlFDB\createSpatialEngine.sql"
  File "bin\sql\sqlFDB\dropSpatialEngine.sql"
  File "bin\sql\sqlFDB\shrinkDatabase.sql"
  
  SetOutPath "$INSTDIR\sql\postgreFDB"
  File "bin\sql\postgreFDB\createdatabase.sql"
  
  SetOutPath "$INSTDIR\sql\SQLiteFDB"
  File "bin\sql\SQLiteFDB\createdatabase.sql"
  
  ; ####################################
  ; xsl
  ; ####################################
  CreateDirectory "$INSTDIR\xsl"
  
  ; ####################################
  ; misc
  ; ####################################
  CreateDirectory "$INSTDIR\misc"
  CreateDirectory "$INSTDIR\misc\tiling"
  CreateDirectory "$INSTDIR\misc\tiling\import"
  CreateDirectory "$INSTDIR\misc\tiling\export"
  
  CreateDirectory "$INSTDIR\misc\wms"
  CreateDirectory "$INSTDIR\misc\wms\GetFeatureInfo"
  CreateDirectory "$INSTDIR\misc\wms\GetFeatureInfo\xsl"
  
  SetOutPath "$INSTDIR\misc\tiling\import"
  File "bin\misc\tiling\import\osm.xml"
  SetOutPath "$INSTDIR\misc\tiling\export"
  File "bin\misc\tiling\export\tilecache.xml"
  File "bin\misc\tiling\export\tilecache_4326.xml"
  File "bin\misc\tiling\export\tilecache_3785.xml"
  
  SetOutPath "$INSTDIR\misc\wms\GetFeatureInfo\xsl"
  File "bin\misc\wms\GetFeatureInfo\xsl\featureinfo_html.xsl"
  
  ; ####################################
  ; x64/x86 -> SQLite
  ; ####################################
  CreateDirectory "$INSTDIR\x86"
  CreateDirectory "$INSTDIR\x86"
  
  SetOutPath "$INSTDIR\x86"
  File "bin\x86\SQLite.Interop.dll"
  SetOutPath "$INSTDIR\x64"
  File "bin\x64\SQLite.Interop.dll"
  
  ; ####################################
  ; $INSTDIR
  ; ####################################
  SetOutPath "$INSTDIR"
  
  File "bin\gView.DataSources.EventTable.dll"
  File "bin\gView.DataSources.EventTable.UI.dll"
  File "bin\gView.DataSources.Fdb.MSAccess.dll"
  File "bin\gView.DataSources.Fdb.MSSql.dll"
  File "bin\gView.DataSources.Fdb.PostgreSql.dll"
  File "bin\gView.DataSources.Fdb.UI.dll"
  File "bin\gView.DataSources.GDAL.dll"
  File "bin\gView.DataSources.GDAL.UI.dll"
  File "bin\gView.DataSources.MSSqlSpatial.dll"
  File "bin\gView.DataSources.MSSqlSpatial.UI.dll"
  File "bin\gView.DataSources.PostGIS.dll"
  File "bin\gView.DataSources.PostGIS.UI.dll"
  File "bin\gView.DataSources.Raster.dll"
  File "bin\gView.Datasources.Raster.UI.dll"
  File "bin\gView.DataSources.Shape.dll"
  File "bin\gView.DataSources.Shape.UI.dll"
  File "bin\gView.DataSources.Fdb.SQLite.dll"
  
  ;File "bin\gView.Cmd.Checkin.exe"
  File "bin\gView.Cmd.copyFeatureClass.exe"
  File "bin\gView.Cmd.CreateAccessFDB.exe"
  File "bin\gView.Cmd.CreateAccessFDBDataset.exe"
  File "bin\gView.Cmd.CreateSqlFDB.exe"
  File "bin\gView.Cmd.CreateSqlFDBDataset.exe"
  File "bin\gView.Cmd.Fdb.exe"
  File "bin\gView.Cmd.Rds.Util.exe"
  File "bin\gView.Cmd.RefreshShapeIndices.exe"
  File "bin\gView.Cmd.ClipCompactTilecache.exe"
  File "bin\gView.Cmd.CompactTileBundle.exe"
  File "bin\gView.Cmd.FillElasticSearch.exe"
  File "bin\ElasticSearch.Net.dll"
  File "bin\Nest.dll"
  File "bin\default.json"
  ;File "bin\loadtilecache.exe"
  File "bin\Credits.xml"
  File "bin\exmenu.xml"
  File ".\img\new\explorer.ico"
  File ".\img\new\explorer16.ico"
  File "bin\gView.Setup.GACInstall.exe"
  File "bin\gView.GPS.dll"
  File "bin\gView.GPS.UI.dll"
  ;File "bin\gv_rds_util.exe"
  ;File "bin\gv_serverutil.exe"
  File "bin\gView.Carto.Rendering.dll"
  File "bin\gView.Carto.Rendering.UI.dll"
  File "bin\gView.Data.Fields.dll"
  File "bin\gView.Data.Fields.UI.dll"
  File "bin\gView.Data.Joins.dll"
  File "bin\gView.Data.Joins.UI.dll"
  File "bin\gView.DB.UI.xml"
  File "bin\gView.Dialogs.dll"
  File "bin\gView.Plugins.Editor.dll"
  File "bin\gView.Explorer.UI.dll"
  File "bin\gView.Globalisation.dll"
  File "bin\gView.Interoperability.ArcXML.dll"
  File "bin\gView.Interoperability.ArcXML.UI.dll"
  File "bin\gView.Interoperability.AGS.dll"
  File "bin\gView.Interoperability.AGS.UI.dll"
  File "bin\gView.Interoperability.OGC.dll"
  File "bin\gView.Interoperability.OGC.UI.dll"
  File "bin\gView.Interoperability.Sde.a10.dll"
  File "bin\gView.Interoperability.Sde.UI.a10.dll"
  File "bin\gView.Interoperability.Misc.dll"
  File "bin\gView.Drawing.Pro.dll"
  File "bin\AForge.dll"
  File "bin\AForge.Imaging.dll"
  File "bin\AForge.Math.dll"
  ;File "bin\gView.License.exe"
  File "bin\gView.MapServer.Lib.UI.dll"
  File "bin\gView.MapServerProcess.xml"
  File "bin\gView.Metadata.dll"
  File "bin\gView.Metadata.UI.dll"
  ; File "bin\gView.Cloud.Core.dll"
  File "bin\gView.Desktop.Offline.Sync.exe"
  File "bin\gView.Offline.UI.dll"
  File "bin\gView.OGC.dll"
  File "bin\gView.OGC.UI.dll"
  ;File "bin\gView.PluginManager.exe"
  File "bin\npgsql.dll"
  File "bin\Mono.Security.dll"
  File "bin\gView.Symbology.dll"
  File "bin\gView.Symbology.UI.dll"
  File "bin\gView.Plugins.Snapping.dll"
  File "bin\gView.Plugins.DbTools.dll"
  File "bin\gView.Plugins.Tools.dll"
  File "bin\gView.Web.dll"
  File "bin\gView.XML2.dll"
  File "bin\gView.DataSources.TileCache.dll"
  File "bin\gView.DataSources.TileCache.UI.dll"
  File "bin\gView.Desktop.Wpf.dll"
  File "bin\AvalonDock.dll"
  File "bin\AvalonDock.Themes.Aero.dll"
  File "bin\AvalonDock.Themes.Expression.dll"
  File "bin\AvalonDock.Themes.Metro.dll"
  File "bin\AvalonDock.Themes.VS2010.dll"
  File "bin\Fluent.dll"
  File "bin\System.Data.SQLite.dll" 
  File "bin\System.Data.SQLite.Linq.dll" 
  ;File "bin\InstallUtil.exe"
  File ".\img\new\map.ico"
  File ".\img\new\map16.ico"
  File "bin\gView.MapServer.Lib.dll"
  File "bin\MapServerConfig.Setup.xml"
  File "bin\menu.carto.xml"
  File "bin\normal.mxl"
  File "bin\gView.Setup.PostInstallation.exe"
  ${If} ${RunningX64}
    File ".\bin64\proj.dll"
    File ".\bin64\msvcr100.dll"
    File ".\bin64\geom.dll"
    File ".\bin64\MrSidLib.dll"
    File ".\bin64\lti_dsdk.dll"
	File ".\bin64\lti_dsdk_9.5.dll"
	File ".\bin64\lti_dsdk_cdll_9.5.dll"
	File ".\bin64\tbb.dll"
  ${Else}
    File ".\bin32\proj.dll"
    File ".\bin32\msvcr71.dll"
    File ".\bin32\msvcr100.dll"
    File ".\bin32\geom.dll"
    File ".\bin32\MrSidLib.dll" 
  ${EndIf}
  File "bin\proj.db"
  File "bin\gView.Network.dll"
  File "bin\gView.Plugins.Network.dll"
  File "bin\sync.xml"
  File "bin\ArcGisFeatureDatabasePlugin10.dll"
  File "bin\ArcGisFeatureDatabasePlugin10.tlb"
  
  File "bin\Newtonsoft.Json.dll"
  ;File "_UninstallBatch.bat"
  ;File "whatsnew.txt"
  
  CreateDirectory "$LOCALAPPDATA\gView"
  CreateDirectory "$LOCALAPPDATA\gView\LogFiles"
  CreateDirectory "$LOCALAPPDATA\gView\MapServer"
  CreateDirectory "$LOCALAPPDATA\gView\MapServer\Services"
  
  ExecWait "$INSTDIR\gView.Setup.GACInstall.exe -i"
  
  SetShellVarContext all

  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
  
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder"
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder\Administration"
  
  CreateShortCut "$SMPROGRAMS\$StartMenuFolder\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  
  CreateShortCut "$SMPROGRAMS\$StartMenuFolder\Administration\Parse Plugins.lnk" "$INSTDIR\gView.Setup.PostInstallation.exe" "-p" "${NSISDIR}\Contrib\Graphics\Icons\nsis1-install.ico" 0
  
  !insertmacro MUI_STARTMENU_WRITE_END
SectionEnd

SectionGroup /e "Applications"

Section "Carto"
  SetOutPath "$INSTDIR"
 	
  File "bin\gView.Desktop.Wpf.Carto.exe"
  
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
  CreateShortCut "$SMPROGRAMS\$StartMenuFolder\Carto.lnk" "$INSTDIR\gView.Desktop.Wpf.Carto.exe" "" "$INSTDIR\gView.Desktop.Wpf.Carto.exe" 0
  !insertmacro MUI_STARTMENU_WRITE_END
  
  ${registerExtension} "$INSTDIR\gView.Desktop.Wpf.Carto.exe" ".mxl" "Map Xml"
SectionEnd

Section "Data Explorer"
  SetOutPath "$INSTDIR"
 	
  File "bin\gView.Desktop.Wpf.DataExplorer.exe"
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
  CreateShortCut "$SMPROGRAMS\$StartMenuFolder\Data Explorer.lnk" "$INSTDIR\gView.Desktop.Wpf.DataExplorer.exe" "" "$INSTDIR\gView.Desktop.Wpf.DataExplorer.exe" 0
  !insertmacro MUI_STARTMENU_WRITE_END
SectionEnd

Section "Map Server"
  SetOutPath "$INSTDIR"
 	
  File "bin\gView.MapServer.Instance.exe"
  File "bin\gView.MapServer.Instance.exe.config"
  File "bin\gView.MapServer.Tasker.exe"
  File "bin\gView.MapServer.Tasker.exe.config"
  File "bin\gView.MapServer.Tasker.Service.dll"
  
  CreateDirectory "$INSTDIR\html"
  SetOutPath "$INSTDIR\html"
  File "bin\html\TaskerIndex.htm"
  File "bin\html\TaskerCatalog.htm"
  File "bin\html\TaskerRecycleAll.htm"
  File "bin\html\TaskerServiceCapabilities.htm"
  File "bin\html\TaskerStyles.css"
  
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
  CreateShortCut "$SMPROGRAMS\$StartMenuFolder\Administration\MapServer.lnk" "$INSTDIR\gView.Desktop.MapServer.Admin.exe" "" "$INSTDIR\gView.Desktop.MapServer.Admin.exe" 0
  !insertmacro MUI_STARTMENU_WRITE_END
SectionEnd
              
SectionGroupEnd



Section "Postinstallation"
  SectionIn RO
  ExecWait "$INSTDIR\gView.Setup.PostInstallation.exe -p"
  ExecWait "$INSTDIR\gView.Desktop.MapServer.Admin.exe -service_install"
  ExecWait "$INSTDIR\gView.Desktop.MapServer.Admin.exe -confirm_open"
SectionEnd

;--------------------------------

; Uninstaller

Section "Uninstall"
  
  ExecWait "$INSTDIR\gView.Desktop.MapServer.Admin.exe -service_uninstall"
  ExecWait "$INSTDIR\gView.Setup.GACInstall.exe -u"
 
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\gViewGISOS"
  ;DeleteRegKey HKLM SOFTWARE\gView

  ; Remove files and uninstaller
  Delete "$INSTDIR\gView.Setup.Uninstall.dll"
  Delete "$INSTDIR\gView.DataSources.EventTable.dll"
  Delete "$INSTDIR\gView.DataSources.EventTable.UI.dll"
  Delete "$INSTDIR\gView.DataSources.Fdb.MSAccess.dll"
  Delete "$INSTDIR\gView.DataSources.Fdb.MSSql.dll"
  Delete "$INSTDIR\gView.DataSources.Fdb.PostgreSql.dll"
  Delete "$INSTDIR\gView.DataSources.Fdb.UI.dll"
  Delete "$INSTDIR\gView.DataSources.GDAL.dll"
  Delete "$INSTDIR\gView.DataSources.GDAL.UI.dll"
  Delete "$INSTDIR\gView.DataSources.MSSqlSpatial.dll"
  Delete "$INSTDIR\gView.DataSources.MSSqlSpatial.UI.dll"
  Delete "$INSTDIR\gView.DataSources.PostGIS.dll"
  Delete "$INSTDIR\gView.DataSources.PostGIS.UI.dll"
  Delete "$INSTDIR\gView.DataSources.Raster.dll"
  Delete "$INSTDIR\gView.Datasources.Raster.UI.dll"
  Delete "$INSTDIR\gView.DataSources.Shape.dll"
  Delete "$INSTDIR\gView.DataSources.Shape.UI.dll"
  Delete "$INSTDIR\gView.DataSources.Fdb.SQLite.dll"
  Delete "$INSTDIR\System.Data.SQLite.dll"
  Delete "$INSTDIR\System.Data.SQLite.Linq.dll"
  
  Delete "$INSTDIR\gView.Cmd.CompactTileBundle.exe"
  Delete "$INSTDIR\gView.Cmd.ClipCompactTileCache.exe"
  Delete "$INSTDIR\gView.Cmd.Checkin.exe"
  Delete "$INSTDIR\gView.Cmd.copyFeatureClass.exe"
  Delete "$INSTDIR\gView.Cmd.CreateAccessFDB.exe"
  Delete "$INSTDIR\gView.Cmd.CreateAccessFDBDataset.exe"
  Delete "$INSTDIR\gView.Cmd.CreateSqlFDB.exe"
  Delete "$INSTDIR\gView.Cmd.CreateSqlFDBDataset.exe"
  Delete "$INSTDIR\gView.Cmd.Fdb.exe"
  Delete "$INSTDIR\gView.Cmd.Rds.Util.exe"
  Delete "$INSTDIR\gView.Cmd.RefreshShapeIndices"
  Delete "$INSTDIR\gView.Cmd.FillElasticSearch.exe"
  Delete "$INSTDIR\ElasticSearch.Net.dll"
  Delete "$INSTDIR\Nest.dll"
  Delete "$INSTDIR\default.json"
  ;Delete "$INSTDIR\loadtilecache.exe"
  Delete "$INSTDIR\Credits.xml"
  Delete "$INSTDIR\menu.explorer.xml"
  Delete "$INSTDIR\explorer.ico"
  Delete "$INSTDIR\explorer16.ico"
  Delete "$INSTDIR\gView.Setup.GACInstall.exe"
  Delete "$INSTDIR\gView.GPS.dll"
  Delete "$INSTDIR\gView.GPS.UI.dll"
  ;Delete "$INSTDIR\gv_rds_util.exe"
  ;Delete "$INSTDIR\gv_serverutil.exe"
  Delete "$INSTDIR\gView.Carto.Rendering.dll"
  Delete "$INSTDIR\gView.Carto.Rendering.UI.dll"
  Delete "$INSTDIR\gView.Data.Fields.dll"
  Delete "$INSTDIR\gView.Data.Fields.UI.dll"
  Delete "$INSTDIR\gView.Data.Joins.dll"
  Delete "$INSTDIR\gView.Data.Joins.UI.dll"
  Delete "$INSTDIR\gView.DB.UI.xml"
  Delete "$INSTDIR\gView.Dialogs.dll"
  Delete "$INSTDIR\gView.Plugins.Editor.dll"
  Delete "$INSTDIR\gView.Explorer.UI.dll"
  Delete "$INSTDIR\gView.Globalisation.dll"
  Delete "$INSTDIR\gView.Interoperability.ArcXML.dll"
  Delete "$INSTDIR\gView.Interoperability.ArcXML.UI.dll"
  Delete "$INSTDIR\gView.Interoperability.AGS.dll"
  Delete "$INSTDIR\gView.Interoperability.AGS.UI.dll"
  Delete "$INSTDIR\gView.Interoperability.OGC.dll"
  Delete "$INSTDIR\gView.Interoperability.OGC.UI.dll"
  Delete "$INSTDIR\gView.Interoperability.Sde.a10.dll"
  Delete "$INSTDIR\gView.Interoperability.Sde.UI.a10.dll"
  Delete "$INSTDIR\gView.Interoperability.Sde.dll"
  Delete "$INSTDIR\gView.Interoperability.Sde.UI.dll"
  Delete "$INSTDIR\gView.Interoperability.Misc.dll"
  ;Delete "$INSTDIR\gView.License.exe"
  Delete "$INSTDIR\gView.MapServer.Lib.UI.dll"
  Delete "$INSTDIR\gView.MapServerProcess.xml"
  Delete "$INSTDIR\gView.Metadata.dll"
  Delete "$INSTDIR\gView.Metadata.UI.dll"
  ; Delete "$INSTDIR\gView.Cloud.Core.dll"
  Delete "$INSTDIR\gView.Desktop.Offline.Sync.exe"
  Delete "$INSTDIR\gView.Offline.UI.dll"
  Delete "$INSTDIR\gView.OGC.dll"
  Delete "$INSTDIR\gView.OGC.UI.dll"
  ;Delete "$INSTDIR\gView.PluginManager.exe"
  Delete "$INSTDIR\npgsql.dll"
  Delete "$INSTDIR\Mono.Security.dll"
  Delete "$INSTDIR\gView.Symbology.dll"
  Delete "$INSTDIR\gView.Symbology.UI.dll"
  Delete "$INSTDIR\gView.Plugins.Snapping.dll"
  Delete "$INSTDIR\gView.Plugins.DbTools.dll"
  Delete "$INSTDIR\gView.Plugins.Tools.dll"
  Delete "$INSTDIR\gView.Web.dll"
  Delete "$INSTDIR\gView.XML2.dll"
  Delete "$INSTDIR\gView.DataSources.TileCache.dll"
  Delete "$INSTDIR\gView.DataSources.TileCache.UI.dll"
  Delete "$INSTDIR\gView.Desktop.Wpf.dll"
  Delete "$INSTDIR\AvalonDock.Themes.Aero.dll"
  Delete "$INSTDIR\AvalonDock.Themes.Expression.dll"
  Delete "$INSTDIR\AvalonDock.Themes.Metro.dll"
  Delete "$INSTDIR\AvalonDock.Themes.VS2010.dll"
  Delete "$INSTDIR\Fluent.dll"
  ;Delete "$INSTDIR\InstallUtil.exe"
  Delete "$INSTDIR\map.ico"
  Delete "$INSTDIR\map16.ico"
  Delete "$INSTDIR\gView.MapServer.Lib.dll"
  Delete "$INSTDIR\menu.xml"
  Delete "$INSTDIR\normal.mxl"
  Delete "$INSTDIR\gView.Setup.PostInstallation.exe"
  Delete "$INSTDIR\Newtonsoft.Json.dll"

  Delete "$INSTDIR\gView.Drawing.Pro.dll"
  Delete "$INSTDIR\AForge.dll"
  Delete "$INSTDIR\AForge.Imaging.dll"
  Delete "$INSTDIR\AForge.Math.dll"
  
    Delete "$INSTDIR\proj.dll"
    Delete "$INSTDIR\msvcr100.dll"
    Delete "$INSTDIR\geom.dll"
    Delete "$INSTDIR\MrSidLib.dll"
    Delete "$INSTDIR\lti_dsdk.dll"
	Delete "$INSTDIR\lti_dsdk_9.5.dll"
	Delete "$INSTDIR\lti_dsdk_cdll_9.5.dll"
	Delete "$INSTDIR\tbb.dll"
    Delete "$INSTDIR\msvcr71.dll"
    Delete "$INSTDIR\msvcr100.dll"

  Delete "$INSTDIR\proj.db"
  Delete "$INSTDIR\gView.Network.dll"
  Delete "$INSTDIR\gView.Plugins.Network.dll"
  Delete "$INSTDIR\sync.xml"
  Delete "$INSTDIR\ArcGisFeatureDatabasePlugin10.dll"
  Delete "$INSTDIR\ArcGisFeatureDatabasePlugin10.tlb"
  ;Delete "$INSTDIR\_UninstallBatch.bat"
  ;Delete "$INSTDIR\whatsnew.txt"
  
  ; sql
  Delete "$INSTDIR\sql\accessFDB\fdb_1_0_0.tpl"
  Delete "$INSTDIR\sql\accessFDB\fdb_1_2_0.tpl"       
  Delete "$INSTDIR\sql\sqlFDB\createdatabase.sql"
  Delete "$INSTDIR\sql\sqlFDB\createdatabase_from_mdf.sql"
  Delete "$INSTDIR\sql\sqlFDB\createSpatialEngine.sql"
  Delete "$INSTDIR\sql\sqlFDB\dropSpatialEngine.sql"
  Delete "$INSTDIR\sql\sqlFDB\shrinkDatabase.sql"
  Delete "$INSTDIR\sql\postgreFDB\createdatabase.sql"
  RMDir "$INSTDIR\sql\accessFDB"
  RMDir "$INSTDIR\sql\sqlFDB"
  RMDir "$INSTDIR\sql\postgreFDB"
  RMDir "$INSTDIR\sql"
  
  ; misc
  Delete "$INSTDIR\misc\tiling\import\osm.xml"
  Delete "$INSTDIR\misc\tiling\export\tilecache.xml"
  Delete "$INSTDIR\misc\tiling\export\tilecache_3785.xml"
  Delete "$INSTDIR\misc\tiling\export\tilecache_4326.xml"
  RMDir "$INSTDIR\misc\tiling"
  RMDir "$INSTDIR\misc"
   
  ; SQLite 
  Delete "$INSTDIR\x86\SQLite.Interop.dll"
  Delete "$INSTDIR\x64\SQLite.Interop.dll"
  RMDir "$INSTDIR\x86"
  RMDir "$INSTDIR\x64"
   
  ; Framework
  Delete "$INSTDIR\Framework\gView.Core.dll"               
  Delete "$INSTDIR\Framework\gView.Carto.dll"              
  Delete "$INSTDIR\Framework\gView.Core.UI.dll"            
  Delete "$INSTDIR\Framework\gView.Data.dll"               
  Delete "$INSTDIR\Framework\gView.DB.dll"                 
  Delete "$INSTDIR\Framework\gView.DB.UI.dll"              
  Delete "$INSTDIR\Framework\gView.Editor.Core.dll"        
  Delete "$INSTDIR\Framework\gView.Geometry.dll"           
  Delete "$INSTDIR\Framework\gView.MapServer.Connector.dll"
  Delete "$INSTDIR\Framework\gView.Math.dll"               
  Delete "$INSTDIR\Framework\gView.Snapping.Core.dll"      
  Delete "$INSTDIR\Framework\gView.system.dll"             
  Delete "$INSTDIR\Framework\gView.system.UI.dll"          
  Delete "$INSTDIR\Framework\gView.UI.dll"                 
  Delete "$INSTDIR\Framework\gView.XML.dll"
  Delete "$INSTDIR\Framework\gView.Offline.dll"   
  RMDir "$INSTDIR\Framework"
  
  ; doc
  Delete "$INSTDIR\doc\LICENSE-crypto.txt"
  Delete "$INSTDIR\doc\LICENSE-dlmalloc.txt"
  Delete "$INSTDIR\doc\LICENSE-gdal.txt"
  Delete "$INSTDIR\doc\LICENSE-Lizardtech.txt"
  Delete "$INSTDIR\doc\LICENSE-AvalonDock.txt"
  Delete "$INSTDIR\doc\LICENSE-Fluent.txt"
  Delete "$INSTDIR\doc\LICENSE.rtf"
  Delete "$INSTDIR\doc\License.txt"
  Delete "$INSTDIR\doc\README-3rdparty.txt"
  Delete "$INSTDIR\doc\LICENSE-npgsql.txt"
  Delete "$INSTDIR\doc\gView 4.0 Handbuch.pdf"
  Delete "$INSTDIR\doc\Whitepaper - gView Daten in ArcMap laden.pdf"
  Delete "$INSTDIR\doc\Whitepaper - Ortsplan erstellen.pdf"
  Delete "$INSTDIR\doc\Whitepaper - Migration von ArcXML Diensten.pdf"
  Delete "$INSTDIR\doc\Whitepaper - Joins & Views.pdf"
  Delete "$INSTDIR\doc\Whitepaper - OGR Simple Feature Library.pdf"
  RMDir "$INSTDIR\doc"
  
  ; de
  Delete "$INSTDIR\de\gView.DataSources.Fdb.UI.resources.dll"
  Delete "$INSTDIR\de\gView.DataSources.PostGIS.UI.resources.dll"
  Delete "$INSTDIR\de\gView.Carto.Rendering.UI.resources.dll"
  Delete "$INSTDIR\de\gView.DB.UI.resources.dll"
  Delete "$INSTDIR\de\gView.Dialogs.resources.dll"
  Delete "$INSTDIR\de\gView.Explorer.UI.resources.dll"
  Delete "$INSTDIR\de\gView.Globalisation.resources.dll"
  Delete "$INSTDIR\de\gView.Interoperability.ArcXML.UI.resources.dll"
  Delete "$INSTDIR\de\gView.Interoperability.AGS.UI.resources.dll"
  Delete "$INSTDIR\de\gView.Interoperability.OGC.UI.resources.dll"
  Delete "$INSTDIR\de\gView.Interoperability.Sde.UI.a10.resources.dll"
  Delete "$INSTDIR\de\gView.MapServer.Lib.UI.resources.dll"
  Delete "$INSTDIR\de\gView.Plugins.Snapping.resources.dll"
  Delete "$INSTDIR\de\gView.Plugins.DbTools.resources.dll"
  Delete "$INSTDIR\de\gView.Plugins.Editor.resources.dll"
  Delete "$INSTDIR\de\gView.Plugins.Tools.resources.dll"
  Delete "$INSTDIR\de\gView.Symbology.UI.resources.dll"
  Delete "$INSTDIR\de\gView.system.UI.resources.dll"
  RMDir "$INSTDIR\de"
   
  ;Cursors
  Delete "$INSTDIR\Cursors\Rotation.cur"
  Delete "$INSTDIR\Cursors\Vertex.cur"
  RMDir "$INSTDIR\Cursors"
  
  ;config
  Delete "$INSTDIR\config\templates\gView.MapServer.Instance.exe.config"
  RMDir "$INSTDIR\config\explorer\tmp"
  RMDir "$INSTDIR\config\explorer"
  RMDir "$INSTDIR\config\templates"
  RMDir "$INSTDIR\config"
  
  ;xsl
  RMDIR "$INSTDIR\xsl"
  
  ;MapServer
  ;Delete "$INSTDIR\_createService.bat"                 
  ;Delete "$INSTDIR\_removeService.bat"               
  Delete "$INSTDIR\gView.MapServer.Instance.exe"                  
  Delete "$INSTDIR\gView.MapServer.Instance.exe.config"           
  Delete "$INSTDIR\gView.MapServer.Tasker.exe"
  Delete "$INSTDIR\gView.MapServer.Tasker.exe.config"
  Delete "$INSTDIR\gView.MapServer.Tasker.Service.dll"
  Delete "$INSTDIR\TaskerIndex.htm"          
  Delete "$INSTDIR\gView.Desktop.MapServer.Admin.exe"
  ;Delete "$INSTDIR\Start_gView_Image_Server.bat"         
  ;Delete "$INSTDIR\Stop_gView_Image_Server.bat"          
  
  ;Explorer
  Delete "$INSTDIR\gView.Desktop.DataExplorer.exe"
  Delete "$INSTDIR\gView.Desktop.Wpf.DataExplorer.exe"
  
  ;Carto
  Delete "$INSTDIR\gView.Desktop.Wpf.Carto.exe"
  
  ${unregisterExtension} ".mxl" "Map Xml"
  
  Delete "$INSTDIR\uninstall.exe"
  
  SetShellVarContext all
  
  ; Remove shortcuts, if any
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
  Delete "$SMPROGRAMS\$StartMenuFolder\Administration\*.*"
  Delete "$SMPROGRAMS\$StartMenuFolder\*.*"
  
  RMDIR "$SMPROGRAMS\$StartMenuFolder\Administration"
  RMDir "$SMPROGRAMS\$StartMenuFolder"
  !insertmacro MUI_STARTMENU_WRITE_END
  ; Remove directories used
  
  RMDir "$LOCALAPPDATA\gView\LogFiles"
  RMDir "$LOCALAPPDATA\gView\MapServer\Services"
  RMDir "$LOCALAPPDATA\gView\MapServer"
  RMDir "$LOCALAPPDATA\gView"
  
  RMDir "$INSTDIR"

SectionEnd
