<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration/system.web">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:if test="count(httpHandlers)=0">
        <httpHandlers>
          <add verb="GET" path="Composite/downloadmediafolder.ashx" type="CompositeC1Contrib.MediaArchiveDownloader.Web.DownloadFolderHandler, CompositeC1Contrib.MediaArchiveDownloader" />
        </httpHandlers>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration/system.web/httpHandlers">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:if test="count(add[@type='CompositeC1Contrib.MediaArchiveDownloader.Web.DownloadFolderHandler, CompositeC1Contrib.MediaArchiveDownloader'])=0">
        <add verb="GET" path="Composite/downloadmediafolder.ashx" type="CompositeC1Contrib.MediaArchiveDownloader.Web.DownloadFolderHandler, CompositeC1Contrib.MediaArchiveDownloader" />
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration/system.webServer">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:if test="count(handlers)=0">
        <handlers>
          <add name="MediaArchiveDownloader" verb="GET" path="Composite/downloadmediafolder.ashx" type="CompositeC1Contrib.MediaArchiveDownloader.Web.DownloadFolderHandler, CompositeC1Contrib.MediaArchiveDownloader" />
        </handlers>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration/system.webServer/handlers">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:if test="count(add[@type='CompositeC1Contrib.MediaArchiveDownloader.Web.DownloadFolderHandler, CompositeC1Contrib.MediaArchiveDownloader'])=0">
        <add name="MediaArchiveDownloader" verb="GET" path="Composite/downloadmediafolder.ashx" type="CompositeC1Contrib.MediaArchiveDownloader.Web.DownloadFolderHandler, CompositeC1Contrib.MediaArchiveDownloader" />
      </xsl:if>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>