<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output omit-xml-declaration="yes"/>
    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()"/>
        </xsl:copy>
    </xsl:template>

    <!-- Deletes all the native clans except for the ids listed below, so it's pretty much left with the royal factions -->
    
    <xsl:template match="Faction[@id='player_faction']">
        <xsl:copy-of select="." />
    </xsl:template>

    <xsl:template match="Faction" />
</xsl:stylesheet>
