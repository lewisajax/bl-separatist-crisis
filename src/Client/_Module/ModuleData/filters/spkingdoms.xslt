<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output omit-xml-declaration="yes"/>
	<xsl:template match="@*|node()">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>

	<!-- Deletes all the native kingdoms except for the ids listed below -->

	<!-- <xsl:template match="Kingdom">
		<xsl:copy-of select="." />
	</xsl:template> -->

	<xsl:template match="Kingdom" />
</xsl:stylesheet>
