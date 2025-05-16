<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output omit-xml-declaration="yes"/>
    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()"/>
        </xsl:copy>
    </xsl:template>

    <!-- Deletes all of the native settlements, then whatever settlement to you to add, you add in SeparatistCrisis/settlements.xml -->
    
    <xsl:template match="Settlement"/>
</xsl:stylesheet>