<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output omit-xml-declaration="yes"/>
    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()"/>
        </xsl:copy>
    </xsl:template>

    <!-- Deletes all the native heroes except for the ids listed below -->

    <xsl:template match="Hero[
        @id='main_hero' or
        @id='main_hero_mother' or
        @id='main_hero_father' or
        @id='dead_lord_6_1' or
        @id='lord_1_1' or
        @id='lord_1_7' or
        @id='lord_1_14' or
        @id='lord_2_1' or
        @id='lord_3_1' or
        @id='lord_4_1' or
        @id='lord_5_1' or
        @id='lord_6_1']">
        <xsl:copy-of select="." />
    </xsl:template>

    <!-- Need to delete the spouse attribute -->

    <xsl:template match="Hero"/>
</xsl:stylesheet>