<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output omit-xml-declaration="yes"/>
    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()"/>
        </xsl:copy>
    </xsl:template>

    <!-- Deletes all the native clans except for the ids listed below, so it's pretty much left with the royal factions -->
    
    <xsl:template match="Faction[
        @id='player_faction' or
        @id='clan_empire_north_1' or
        @id='clan_empire_west_1' or
        @id='clan_empire_south_1' or
        @id='clan_sturgia_1' or
        @id='clan_aserai_1' or
        @id='clan_vlandia_1' or
        @id='clan_battania_1' or
        @id='clan_khuzait_1']">
        <xsl:copy-of select="." />
    </xsl:template>

    <xsl:template match="Faction" />
</xsl:stylesheet>
