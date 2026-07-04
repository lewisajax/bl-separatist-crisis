<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output omit-xml-declaration="yes"/>
    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()"/>
        </xsl:copy>
    </xsl:template>

	<xsl:template match="Culture[
				  @id='neutral_culture' or
				  @id='empire' or
				  @id='aserai' or
				  @id='sturgia' or
				  @id='vlandia' or
				  @id='battania' or
				  @id='khuzait' or
				  @id='nord' or 
				  @id='vakken' or
				  @id='darshi' or
				  @id='looters' or
				  @id='sea_raiders' or
				  @id='mountain_bandits' or
				  @id='forest_bandits' or
				  @id='desert_bandits' or
				  @id='steppe_bandits']" >
		<xsl:copy-of select="." />
	</xsl:template>

	<xsl:template match="lord_templates/template" />
	<xsl:template match="rebellion_hero_templates/template" />
</xsl:stylesheet>