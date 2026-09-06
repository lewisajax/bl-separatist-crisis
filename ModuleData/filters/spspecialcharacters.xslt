<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output omit-xml-declaration="yes"/>
    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()"/>
        </xsl:copy>
    </xsl:template>
	
	<!-- It's the culture/lord_templates that is the issue -->

    <!-- Deletes all the native lord template characters except for the ids listed below -->
	<xsl:template match="NPCCharacter[@occupation='Lord' and @is_template='true']" />
</xsl:stylesheet>