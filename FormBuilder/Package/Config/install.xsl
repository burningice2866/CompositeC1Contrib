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

      <xsl:if test="count(add[@name='FormBuilderElementProvider'])=0">
        <add type="CompositeC1Contrib.FormBuilder.ElementProviders.FormBuilderElementProvider, CompositeC1Contrib.FormBuilder" name="FormBuilderElementProvider" />
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration/Composite.C1Console.Elements.Plugins.ElementProviderConfiguration/ElementProviderPlugins/add[@name='VirtualElementProvider']/VirtualElements">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:if test="count(add[@name='FormBuilderPerspective'])=0">
        <xsl:variable name="label">
          <xsl:text disable-output-escaping="yes">
            ${Composite.Management, VirtualElementProviderElementProvider.ContentPerspective}
          </xsl:text>
        </xsl:variable>

        <add id="ContentPerspective" order="10" parentId="ID01" tag="Forms" providerName="FormBuilderElementProvider" name="FormBuilderPerspective" label="${label}" closeFolderIconName="" openFolderIconName="Composite.Icons.perspective-content" type="Composite.Plugins.Elements.ElementProviders.VirtualElementProvider.ProviderHookingElementConfigurationElement, Composite" />
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/configuration/Composite.C1Console.Elements.Plugins.ElementActionProviderConfiguration/ElementActionProviderPlugins">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />

      <xsl:if test="count(add[@name='CompositeC1Contrib.FormBuilder'])=0">
        <add name="CompositeC1Contrib.FormBuilder" type="CompositeC1Contrib.FormBuilder.FormBuilderActionProvider, CompositeC1Contrib.FormBuilder" />
      </xsl:if>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>