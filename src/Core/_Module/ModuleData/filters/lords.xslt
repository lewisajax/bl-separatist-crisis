<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output omit-xml-declaration="yes"/>
    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()"/>
        </xsl:copy>
    </xsl:template>

    <!-- Deletes all the native lords except for the ids listed below -->

    <xsl:template match="NPCCharacter[@id='main_hero']" >
        <xsl:copy-of select="." />
    </xsl:template>

	<!-- and is_template!='true' - is_template was removed and I can't remember why we needed to do this -->
    <xsl:template match="NPCCharacter[@occupation='Lord' and @id!='main_hero']" />
</xsl:stylesheet>