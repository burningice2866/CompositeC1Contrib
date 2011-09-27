<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration/Composite.C1Console.Elements.Plugins.ElementProviderConfiguration/ElementProviderPlugins">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:if test="count(add[@name='EmailElementProvider'])=0">
        <add type="CompositeC1Contrib.Email.ElementProviders.EmailElementProvider, CompositeC1Contrib.Email" name="EmailElementProvider" />
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration/Composite.C1Console.Elements.Plugins.ElementProviderConfiguration/ElementProviderPlugins/add[@name='VirtualElementProvider']/VirtualElements">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:if test="count(add[@name='SystemPerspectiveEmail'])=0">
        <xsl:variable name="label">
          <xsl:text disable-output-escaping="yes">
            ${Composite.Management, VirtualElementProviderElementProvider.SystemPerspective}
          </xsl:text>
        </xsl:variable>
        
        <add id="SystemPerspective" order="99" parentId="ID01" tag="System" providerName="EmailElementProvider" label="{$label}" closeFolderIconName="Composite.Icons.perspective-system" openFolderIconName="" type="Composite.Plugins.Elements.ElementProviders.VirtualElementProvider.ProviderHookingElementConfigurationElement, Composite" name="SystemPerspectiveEmail" />
      </xsl:if>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>