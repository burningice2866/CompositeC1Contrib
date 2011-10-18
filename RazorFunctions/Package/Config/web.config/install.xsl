<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration/configSections">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:if test="count(sectionGroup[@name='system.web.webPages.razor'])=0">
        <sectionGroup name="system.web.webPages.razor" type="System.Web.WebPages.Razor.Configuration.RazorWebSectionGroup, System.Web.WebPages.Razor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35">
          <section name="host" type="System.Web.WebPages.Razor.Configuration.HostSection, System.Web.WebPages.Razor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" requirePermission="false" />
          <section name="pages" type="System.Web.WebPages.Razor.Configuration.RazorPagesSection, System.Web.WebPages.Razor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" requirePermission="false" />
        </sectionGroup>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:if test="count(configSections)=0">
        <configSections>
          <sectionGroup name="system.web.webPages.razor" type="System.Web.WebPages.Razor.Configuration.RazorWebSectionGroup, System.Web.WebPages.Razor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35">
            <section name="host" type="System.Web.WebPages.Razor.Configuration.HostSection, System.Web.WebPages.Razor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" requirePermission="false" />
            <section name="pages" type="System.Web.WebPages.Razor.Configuration.RazorPagesSection, System.Web.WebPages.Razor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" requirePermission="false" />
          </sectionGroup>
        </configSections>
      </xsl:if>

      <xsl:if test="count(system.web.webPages.razor)=0">
        <system.web.webPages.razor>
          <host factoryType="CompositeC1Contrib.RazorFunctions.RazorHostFactory"/>
          <pages pageBaseType="CompositeC1Contrib.RazorFunctions.CompositeC1WebPage">
            <namespaces>
              <add namespace="System" />
              <add namespace="System.Linq" />
              <add namespace="System.Web.WebPages.Html" />

              <add namespace="Composite.Data" />
              <add namespace="Composite.Data.Types" />
            </namespaces>
          </pages>
        </system.web.webPages.razor>
      </xsl:if>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>