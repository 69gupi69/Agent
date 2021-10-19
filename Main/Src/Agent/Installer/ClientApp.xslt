<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:wix="http://schemas.microsoft.com/wix/2006/wi"
  xmlns="http://schemas.microsoft.com/wix/2006/wi"
  exclude-result-prefixes="wix">
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="wix:Wix">

    <!--Выбираем все кроме *.pdb-->
    <xsl:variable name="ClientApp" 
                  select="wix:Fragment/wix:DirectoryRef/wix:Component[
                  not(substring(wix:File/@Source, string-length(wix:File/@Source) - 3) = '.pdb') and
                  not(substring(wix:File/@Source, string-length(wix:File/@Source) - 2) = '.db') and
                  not(substring(wix:File/@Source, string-length(wix:File/@Source) - 3) = '.log')
                  ]"/>
    <Wix>
      <Fragment>
        <DirectoryRef Id="InstallLocation">
          <xsl:apply-templates select="$ClientApp"/>
        </DirectoryRef>
      </Fragment>
      <Fragment>
        <ComponentGroup Id="ClientAppLibraries">
          <xsl:for-each select="$ClientApp">
            <ComponentRef Id="{@Id}"/>
          </xsl:for-each>
        </ComponentGroup>
      </Fragment>
    </Wix>
  </xsl:template>


  <xsl:template match="wix:Component">
    <xsl:copy-of select="."/>
  </xsl:template>

</xsl:stylesheet>